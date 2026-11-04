using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public class UpdateStatusDto
    {
        [Required]
        public RequestStatus? Status { get; set; }

        [Required]
        public byte[]? RowVersion { get; set; }
    }
}
