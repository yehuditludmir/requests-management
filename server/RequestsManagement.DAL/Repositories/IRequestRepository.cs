using RequestsManagement.DAL.Entities;
using RequestsManagement.DTO;

namespace RequestsManagement.DAL.Repositories
{
    public interface IRequestRepository
    {
        Task<PagedResultDto<RequestDto>> GetPageAsync(RequestQueryDto query, CancellationToken cancellationToken);

        Task<RequestStatsDto> GetStatsAsync(RequestFilterDto filter, CancellationToken cancellationToken);

        Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<Dictionary<int, Request>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<RequestStatusChangeDto>> GetHistoryAsync(int requestId, CancellationToken cancellationToken);

        void AddStatusChange(RequestStatusChange change);

        Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
    }
}
