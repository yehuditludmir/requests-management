using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public record BulkUpdateFailureDto(int Id, UpdateStatusOutcome Reason, string Error);
}
