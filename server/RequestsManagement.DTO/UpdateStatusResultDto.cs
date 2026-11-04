using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public record UpdateStatusResultDto(UpdateStatusOutcome Outcome, RequestDto? Request = null, string? Error = null);
}
