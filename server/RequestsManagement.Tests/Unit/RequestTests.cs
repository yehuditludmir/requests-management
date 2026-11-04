using RequestsManagement.DTO.Enums;

namespace RequestsManagement.Tests.Unit
{
    public class RequestTests
    {
        private static readonly DateTime Now = new(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);

        [Theory]
        [InlineData(RequestStatus.New, RequestStatus.InProgress)]
        [InlineData(RequestStatus.InProgress, RequestStatus.Waiting)]
        [InlineData(RequestStatus.InProgress, RequestStatus.Completed)]
        [InlineData(RequestStatus.Waiting, RequestStatus.Completed)]
        [InlineData(RequestStatus.Completed, RequestStatus.InProgress)]
        public void TryChangeStatus_AllowedTransition_ChangesStatusAndUpdatedAt(RequestStatus from, RequestStatus to)
        {
            var request = TestData.NewRequest("Org", "Title", from);

            var changed = request.TryChangeStatus(to, Now);

            Assert.True(changed);
            Assert.Equal(to, request.Status);
            Assert.Equal(Now, request.UpdatedAt);
        }

        [Theory]
        [InlineData(RequestStatus.New, RequestStatus.Completed)]
        [InlineData(RequestStatus.InProgress, RequestStatus.New)]
        [InlineData(RequestStatus.Waiting, RequestStatus.Waiting)]
        [InlineData(RequestStatus.Completed, RequestStatus.New)]
        public void TryChangeStatus_DisallowedTransition_LeavesRequestUnchanged(RequestStatus from, RequestStatus to)
        {
            var request = TestData.NewRequest("Org", "Title", from);
            var originalUpdatedAt = request.UpdatedAt;

            var changed = request.TryChangeStatus(to, Now);

            Assert.False(changed);
            Assert.Equal(from, request.Status);
            Assert.Equal(originalUpdatedAt, request.UpdatedAt);
        }
    }
}
