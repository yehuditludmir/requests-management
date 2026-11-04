using System.ComponentModel.DataAnnotations;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.Tests.Unit
{
    public class BulkUpdateStatusDtoTests
    {
        [Fact]
        public void Validate_DuplicateIds_ReturnsErrorOnItems()
        {
            var dto = new BulkUpdateStatusDto
            {
                Status = RequestStatus.InProgress,
                Items = new()
                {
                    new() { Id = 7, RowVersion = new byte[] { 1 } },
                    new() { Id = 7, RowVersion = new byte[] { 2 } }
                }
            };
            var errors = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), errors, validateAllProperties: true);

            Assert.False(isValid);
            var error = Assert.Single(errors);
            Assert.Equal("Duplicate ids: 7", error.ErrorMessage);
            Assert.Contains(nameof(BulkUpdateStatusDto.Items), error.MemberNames);
        }
    }
}
