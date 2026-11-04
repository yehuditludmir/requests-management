using RequestsManagement.DTO;

namespace RequestsManagement.BL.Services
{
    public interface IRequestService
    {
        Task<PagedResultDto<RequestDto>> GetPageAsync(RequestQueryDto query, CancellationToken cancellationToken);

        Task<RequestStatsDto> GetStatsAsync(RequestFilterDto filter, CancellationToken cancellationToken);

        Task<UpdateStatusResultDto> UpdateStatusAsync(int id, UpdateStatusDto update, string changedBy, CancellationToken cancellationToken);

        Task<BulkUpdateStatusResultDto> BulkUpdateStatusAsync(BulkUpdateStatusDto update, string changedBy, CancellationToken cancellationToken);

        Task<IReadOnlyList<RequestStatusChangeDto>?> GetHistoryAsync(int id, CancellationToken cancellationToken);
    }
}
