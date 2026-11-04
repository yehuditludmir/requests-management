using System.Text.Json;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace RequestsManagement.BL.Services
{
    public class CachedRequestService : IRequestService
    {
        private static readonly TimeSpan StatsExpiration = TimeSpan.FromSeconds(60);

        private static long _statsVersion;

        private readonly IRequestService _inner;
        private readonly IMemoryCache _cache;

        public CachedRequestService(RequestService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public Task<PagedResultDto<RequestDto>> GetPageAsync(RequestQueryDto query, CancellationToken cancellationToken)
            => _inner.GetPageAsync(query, cancellationToken);

        // מחזיר את הסיכומים מה-Cache, ואם אינם שם, טוען אותם ושומר
        public async Task<RequestStatsDto> GetStatsAsync(RequestFilterDto filter, CancellationToken cancellationToken)
        {
            var key = $"stats:v{Interlocked.Read(ref _statsVersion)}:{JsonSerializer.Serialize(filter)}";
            var stats = await _cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = StatsExpiration;
                entry.Size = 1;
                return _inner.GetStatsAsync(filter, cancellationToken);
            });

            return stats!;
        }

        public async Task<UpdateStatusResultDto> UpdateStatusAsync(int id, UpdateStatusDto update, string changedBy, CancellationToken cancellationToken)
        {
            var result = await _inner.UpdateStatusAsync(id, update, changedBy, cancellationToken);

            if (result.Outcome == UpdateStatusOutcome.Updated)
            {
                InvalidateStats();
            }

            return result;
        }

        public async Task<BulkUpdateStatusResultDto> BulkUpdateStatusAsync(BulkUpdateStatusDto update, string changedBy, CancellationToken cancellationToken)
        {
            var result = await _inner.BulkUpdateStatusAsync(update, changedBy, cancellationToken);

            if (result.Succeeded.Count > 0)
            {
                InvalidateStats();
            }

            return result;
        }

        public Task<IReadOnlyList<RequestStatusChangeDto>?> GetHistoryAsync(int id, CancellationToken cancellationToken)
            => _inner.GetHistoryAsync(id, cancellationToken);

        // מבטל את כל הסיכומים השמורים על ידי העלאת מספר הגרסה שבמפתח
        private static void InvalidateStats() => Interlocked.Increment(ref _statsVersion);
    }
}
