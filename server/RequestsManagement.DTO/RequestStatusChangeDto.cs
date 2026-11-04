using RequestsManagement.DTO.Enums;


namespace RequestsManagement.DTO
{
    public record RequestStatusChangeDto(
        RequestStatus PreviousStatus,
        RequestStatus NewStatus,
        DateTime ChangedAt,
        string ChangedBy);
}
