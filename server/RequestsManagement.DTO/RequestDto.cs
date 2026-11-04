using RequestsManagement.DTO.Enums;


namespace RequestsManagement.DTO
{
    public record RequestDto(
        int Id,
        string Title,
        string OrganizationName,
        RequestStatus Status,
        RequestPriority Priority,
        string? AssignedTo,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        byte[] RowVersion);
}
