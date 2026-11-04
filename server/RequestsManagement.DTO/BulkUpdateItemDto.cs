using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.DTO
{
    public class BulkUpdateItemDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required]
        public byte[]? RowVersion { get; set; }
    }
}
