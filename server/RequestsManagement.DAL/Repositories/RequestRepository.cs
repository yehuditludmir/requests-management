using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RequestsManagement.DAL.Data;
using RequestsManagement.DAL.Entities;
using RequestsManagement.DAL.Mapping;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DAL.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _db;

        public RequestRepository(AppDbContext db)
        {
            _db = db;
        }

        // מחזיר עמוד של פניות מסוננות וממוינות, יחד עם המספר הכולל של הפניות המתאימות
        public async Task<PagedResultDto<RequestDto>> GetPageAsync(RequestQueryDto query, CancellationToken cancellationToken)
        {
            var filtered = ApplyFilter(_db.Requests.AsNoTracking(), query);

            var totalCount = await filtered.CountAsync(cancellationToken);

            var items = await ApplySort(filtered, query.SortBy, query.SortDirection)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(RequestMapping.ToDto)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<RequestDto>(items, query.Page, query.PageSize, totalCount);
        }

        // מחזיר ספירת פניות לפי סטטוס ולפי עדיפות. ערך בלי פניות מקבל 0
        public async Task<RequestStatsDto> GetStatsAsync(RequestFilterDto filter, CancellationToken cancellationToken)
        {
            var filtered = ApplyFilter(_db.Requests.AsNoTracking(), filter);

            var byStatus = await filtered
                .GroupBy(r => r.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

            var byPriority = await filtered
                .GroupBy(r => r.Priority)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

            return new RequestStatsDto(
                Enum.GetValues<RequestStatus>().ToDictionary(s => s, s => byStatus.GetValueOrDefault(s)),
                Enum.GetValues<RequestPriority>().ToDictionary(p => p, p => byPriority.GetValueOrDefault(p)));
        }

        public Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            _db.Requests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        public Task<Dictionary<int, Request>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
            _db.Requests
                .Where(r => ids.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, cancellationToken);

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
            _db.Requests.AnyAsync(r => r.Id == id, cancellationToken);

        public async Task<IReadOnlyList<RequestStatusChangeDto>> GetHistoryAsync(int requestId, CancellationToken cancellationToken) =>
            await _db.RequestStatusChanges
                .AsNoTracking()
                .Where(c => c.RequestId == requestId)
                .OrderByDescending(c => c.ChangedAt)
                .ThenByDescending(c => c.Id)
                .Select(c => new RequestStatusChangeDto(c.PreviousStatus, c.NewStatus, c.ChangedAt, c.ChangedBy))
                .ToListAsync(cancellationToken);

        public void AddStatusChange(RequestStatusChange change) => _db.RequestStatusChanges.Add(change);

        // שומר את השינויים. מחזיר false אם הייתה התנגשות (מישהו אחר עדכן את הפנייה בינתיים)
        public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _db.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).ToList())
                {
                    entry.State = EntityState.Detached;
                }
                return false;
            }
        }

        // מוסיף לשאילתה את תנאי הסינון והחיפוש שנשלחו. משותף לרשימה ולסיכומים
        private static IQueryable<Request> ApplyFilter(IQueryable<Request> source, RequestFilterDto filter)
        {
            if (filter.Status is not null)
            {
                source = source.Where(r => r.Status == filter.Status);
            }

            if (filter.Priority is not null)
            {
                source = source.Where(r => r.Priority == filter.Priority);
            }

            if (!string.IsNullOrWhiteSpace(filter.OrganizationName))
            {
                source = source.Where(r => r.OrganizationName == filter.OrganizationName);
            }

            if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
            {
                source = source.Where(r => r.AssignedTo == filter.AssignedTo);
            }

            if (filter.CreatedFrom is not null)
            {
                source = source.Where(r => r.CreatedAt >= filter.CreatedFrom);
            }

            if (filter.CreatedTo is not null)
            {
                var toExclusive = filter.CreatedTo.Value.Date.AddDays(1);
                source = source.Where(r => r.CreatedAt < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                source = source.Where(r => r.Title.Contains(term) || r.OrganizationName.Contains(term));
            }

            return source;
        }

        // מוסיף לשאילתה מיון לפי השדה והכיוון שנבחרו, ואחריו מיון משני לפי Id
        private static IQueryable<Request> ApplySort(IQueryable<Request> source, RequestSortField sortBy, SortDirection direction)
        {
            var desc = direction == SortDirection.Desc;

            var ordered = sortBy switch
            {
                RequestSortField.UpdatedAt => Order(source, r => r.UpdatedAt, desc),
                RequestSortField.Title => Order(source, r => r.Title, desc),
                RequestSortField.OrganizationName => Order(source, r => r.OrganizationName, desc),
                RequestSortField.Priority => Order(source, r => r.Priority, desc),
                RequestSortField.Status => Order(source, r => r.Status, desc),
                _ => Order(source, r => r.CreatedAt, desc)
            };

            return desc ? ordered.ThenByDescending(r => r.Id) : ordered.ThenBy(r => r.Id);
        }

        private static IOrderedQueryable<Request> Order<TKey>(
            IQueryable<Request> source, Expression<Func<Request, TKey>> key, bool desc)
            => desc ? source.OrderByDescending(key) : source.OrderBy(key);
    }
}
