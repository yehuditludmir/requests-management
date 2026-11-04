using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public class RequestFilterDto : IValidatableObject
    {
        public RequestStatus? Status { get; set; }

        public RequestPriority? Priority { get; set; }

        [MaxLength(200)]
        public string? OrganizationName { get; set; }

        [MaxLength(100)]
        public string? AssignedTo { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }

        [MaxLength(100)]
        public string? Search { get; set; }

        // בודק שטווח התאריכים תקין
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CreatedFrom > CreatedTo)
            {
                yield return new ValidationResult(
                    "createdFrom must be earlier than or equal to createdTo",
                    new[] { nameof(CreatedFrom), nameof(CreatedTo) });
            }
        }
    }
}
