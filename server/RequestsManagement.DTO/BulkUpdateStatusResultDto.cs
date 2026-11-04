namespace RequestsManagement.DTO
{
    public record BulkUpdateStatusResultDto(IReadOnlyList<RequestDto> Succeeded, IReadOnlyList<BulkUpdateFailureDto> Failed);
}
