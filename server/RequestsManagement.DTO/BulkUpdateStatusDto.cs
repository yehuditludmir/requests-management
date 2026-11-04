using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public class BulkUpdateStatusDto : IValidatableObject
    {
        public const int MaxItems = 100;
        private const string ItemsCountMessage = "Items must contain between 1 and 100 requests.";

        [Required]
        public RequestStatus? Status { get; set; }

        [Required, MinLength(1, ErrorMessage = ItemsCountMessage), MaxLength(MaxItems, ErrorMessage = ItemsCountMessage)]
        public List<BulkUpdateItemDto>? Items { get; set; }

        // דוחה בקשה שבה אותה פנייה מופיעה יותר מפעם אחת
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var duplicates = Items?.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates is { Count: > 0 })
            {
                yield return new ValidationResult(
                    $"Duplicate ids: {string.Join(", ", duplicates)}", new[] { nameof(Items) });
            }
        }
    }
}
