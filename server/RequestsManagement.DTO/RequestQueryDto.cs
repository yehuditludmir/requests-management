using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DTO
{
    public class RequestQueryDto : RequestFilterDto
    {
        public const int MaxPageSize = 100;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize)]
        public int PageSize { get; set; } = 20;

        public RequestSortField SortBy { get; set; } = RequestSortField.CreatedAt;

        public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    }
}
