using Microsoft.Extensions.Logging;
using RequestsManagement.DAL.Entities;
using RequestsManagement.DAL.Mapping;
using RequestsManagement.DAL.Repositories;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.BL.Services
{
    public class RequestService : IRequestService
    {
        private const string ConflictMessage = "The request was modified by someone else. Reload and try again.";

        private readonly IRequestRepository _repository;
        private readonly ILogger<RequestService> _logger;

        public RequestService(IRequestRepository repository, ILogger<RequestService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<PagedResultDto<RequestDto>> GetPageAsync(RequestQueryDto query, CancellationToken cancellationToken) =>
            _repository.GetPageAsync(query, cancellationToken);

        public Task<RequestStatsDto> GetStatsAsync(RequestFilterDto filter, CancellationToken cancellationToken) =>
            _repository.GetStatsAsync(filter, cancellationToken);

        public async Task<UpdateStatusResultDto> UpdateStatusAsync(int id, UpdateStatusDto update, string changedBy, CancellationToken cancellationToken)
        {
            var request = await _repository.GetByIdAsync(id, cancellationToken);
            return await ApplyStatusChangeAsync(request, update.RowVersion!, update.Status!.Value, changedBy, cancellationToken);
        }

        // מעדכן סטטוס של כמה פניות. כל פנייה נשמרת בנפרד, וכישלון של אחת לא עוצר את האחרות
        public async Task<BulkUpdateStatusResultDto> BulkUpdateStatusAsync(BulkUpdateStatusDto update, string changedBy, CancellationToken cancellationToken)
        {
            var ids = update.Items!.Select(i => i.Id).ToList();
            var requests = await _repository.GetByIdsAsync(ids, cancellationToken);

            var succeeded = new List<RequestDto>();
            var failed = new List<BulkUpdateFailureDto>();

            foreach (var item in update.Items!)
            {
                var result = await ApplyStatusChangeAsync(
                    requests.GetValueOrDefault(item.Id), item.RowVersion!, update.Status!.Value, changedBy, cancellationToken);

                if (result.Outcome == UpdateStatusOutcome.Updated)
                {
                    succeeded.Add(result.Request!);
                }
                else
                {
                    failed.Add(new BulkUpdateFailureDto(item.Id, result.Outcome, result.Error!));
                }
            }

            _logger.LogInformation("Bulk status update to {Status}: {Succeeded} succeeded, {Failed} failed",
                update.Status, succeeded.Count, failed.Count);

            return new BulkUpdateStatusResultDto(succeeded, failed);
        }

        public async Task<IReadOnlyList<RequestStatusChangeDto>?> GetHistoryAsync(int id, CancellationToken cancellationToken)
        {
            if (!await _repository.ExistsAsync(id, cancellationToken))
            {
                return null;
            }

            return await _repository.GetHistoryAsync(id, cancellationToken);
        }

        // בודק ומבצע שינוי סטטוס של פנייה אחת (קיום, התנגשות, מעבר מותר), ושומר אותו יחד עם שורת היסטוריה.
        // משותף לעדכון בודד ול-Bulk
        private async Task<UpdateStatusResultDto> ApplyStatusChangeAsync(
            Request? request, byte[] rowVersion, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return new UpdateStatusResultDto(UpdateStatusOutcome.NotFound, Error: "Request not found.");
            }

            if (!request.RowVersion.SequenceEqual(rowVersion))
            {
                return new UpdateStatusResultDto(UpdateStatusOutcome.Conflict, Error: ConflictMessage);
            }

            var previousStatus = request.Status;
            var now = DateTime.UtcNow;

            if (!request.TryChangeStatus(newStatus, now))
            {
                var allowed = string.Join(", ", RequestStatusTransitions.AllowedFrom(previousStatus));
                return new UpdateStatusResultDto(UpdateStatusOutcome.InvalidTransition,
                    Error: $"Cannot change status from {previousStatus} to {newStatus}. Allowed: {allowed}");
            }

            _repository.AddStatusChange(new RequestStatusChange
            {
                RequestId = request.Id,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedAt = now,
                ChangedBy = changedBy
            });

            if (!await _repository.TrySaveChangesAsync(cancellationToken))
            {
                _logger.LogWarning("Concurrency conflict on save for request {RequestId}", request.Id);
                return new UpdateStatusResultDto(UpdateStatusOutcome.Conflict, Error: ConflictMessage);
            }

            _logger.LogInformation("Request {RequestId} status changed from {PreviousStatus} to {NewStatus} by {ChangedBy}",
                request.Id, previousStatus, newStatus, changedBy);

            return new UpdateStatusResultDto(UpdateStatusOutcome.Updated, RequestMapping.ToDtoCompiled(request));
        }
    }
}
