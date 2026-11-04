using RequestsManagement.DTO.Enums;


namespace RequestsManagement.DTO
{
    public record RequestStatsDto(
        IReadOnlyDictionary<RequestStatus, int> ByStatus,
        IReadOnlyDictionary<RequestPriority, int> ByPriority);
}
