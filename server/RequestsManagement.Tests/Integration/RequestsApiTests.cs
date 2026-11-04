using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RequestsManagement.DTO;
using RequestsManagement.DTO.Enums;
using static RequestsManagement.Tests.TestData;

namespace RequestsManagement.Tests.Integration
{
    [Collection(nameof(ApiCollection))]
    public class RequestsApiTests
    {
        private const string User = "tester";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly ApiFactory _factory;
        private readonly HttpClient _client;

        public RequestsApiTests(ApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetRequests_FilterAndSort_ReturnsOnlyMatchingRequestsInOrder()
        {
            var organization = NewOrganization();
            await _factory.AddRequestsAsync(
                NewRequest(organization, "B title", RequestStatus.New),
                NewRequest(organization, "A title", RequestStatus.New),
                NewRequest(organization, "C title", RequestStatus.Completed));

            var page = await _client.GetFromJsonAsync<PagedResultDto<RequestDto>>(
                $"/api/requests?organizationName={organization}&status=New&sortBy=Title&sortDirection=Asc", JsonOptions);

            Assert.Equal(2, page!.TotalCount);
            Assert.Equal(new[] { "A title", "B title" }, page.Items.Select(r => r.Title));
        }

        [Fact]
        public async Task UpdateStatus_ValidRequest_Returns200AndWritesHistory()
        {
            var request = NewRequest(NewOrganization(), "Title", RequestStatus.New);
            await _factory.AddRequestsAsync(request);

            var response = await PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<RequestDto>(JsonOptions);
            Assert.Equal(RequestStatus.InProgress, updated!.Status);
            Assert.NotEqual(request.RowVersion, updated.RowVersion);

            var history = await GetHistoryAsync(request.Id);
            var change = Assert.Single(history);
            Assert.Equal(RequestStatus.New, change.PreviousStatus);
            Assert.Equal(RequestStatus.InProgress, change.NewStatus);
            Assert.Equal(User, change.ChangedBy);
        }

        [Fact]
        public async Task UpdateStatus_StaleRowVersion_Returns409AndKeepsFirstUpdate()
        {
            var request = NewRequest(NewOrganization(), "Title", RequestStatus.New);
            await _factory.AddRequestsAsync(request);
            var versionBothUsersSaw = request.RowVersion;

            var first = await PatchStatusAsync(request.Id, RequestStatus.InProgress, versionBothUsersSaw);
            var second = await PatchStatusAsync(request.Id, RequestStatus.InProgress, versionBothUsersSaw);

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

            var history = await GetHistoryAsync(request.Id);
            Assert.Single(history);
        }

        [Theory]
        [InlineData("pageSize=0")]
        [InlineData("pageSize=101")]
        [InlineData("sortBy=NotAField")]
        [InlineData("status=NotAStatus")]
        [InlineData("createdFrom=2026-03-01&createdTo=2026-01-01")]
        public async Task GetRequests_InvalidQuery_Returns400(string queryString)
        {
            var response = await _client.GetAsync($"/api/requests?{queryString}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetStats_AfterStatusUpdate_ReflectsTheChange()
        {
            var organization = NewOrganization();
            var request = NewRequest(organization, "Title", RequestStatus.New);
            await _factory.AddRequestsAsync(request, NewRequest(organization, "Other", RequestStatus.New));

            var before = await GetStatsAsync(organization);
            await PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion);
            var after = await GetStatsAsync(organization);

            Assert.Equal(2, before.ByStatus[RequestStatus.New]);
            Assert.Equal(1, after.ByStatus[RequestStatus.New]);
            Assert.Equal(1, after.ByStatus[RequestStatus.InProgress]);
        }

        [Fact]
        public async Task BulkUpdateStatus_MixedItems_ReturnsPartialSuccess()
        {
            var organization = NewOrganization();
            var valid = NewRequest(organization, "Valid", RequestStatus.New);
            var sameStatus = NewRequest(organization, "Already in progress", RequestStatus.InProgress);
            var stale = NewRequest(organization, "Stale version", RequestStatus.New);
            await _factory.AddRequestsAsync(valid, sameStatus, stale);

            var response = await PatchAsync("/api/requests/status", new
            {
                status = RequestStatus.InProgress,
                items = new[]
                {
                    new { id = valid.Id, rowVersion = valid.RowVersion },
                    new { id = sameStatus.Id, rowVersion = sameStatus.RowVersion },
                    new { id = stale.Id, rowVersion = valid.RowVersion },
                    new { id = int.MaxValue, rowVersion = valid.RowVersion }
                }
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BulkUpdateStatusResultDto>(JsonOptions);

            Assert.Equal(new[] { valid.Id }, result!.Succeeded.Select(r => r.Id));
            var reasons = result.Failed.ToDictionary(f => f.Id, f => f.Reason);
            Assert.Equal(3, reasons.Count);
            Assert.Equal(UpdateStatusOutcome.InvalidTransition, reasons[sameStatus.Id]);
            Assert.Equal(UpdateStatusOutcome.Conflict, reasons[stale.Id]);
            Assert.Equal(UpdateStatusOutcome.NotFound, reasons[int.MaxValue]);
        }

        private Task<HttpResponseMessage> PatchStatusAsync(int id, RequestStatus status, byte[] rowVersion) =>
            PatchAsync($"/api/requests/{id}/status", new { status, rowVersion });

        private Task<HttpResponseMessage> PatchAsync(string url, object body)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            message.Headers.Add("X-User", User);
            return _client.SendAsync(message);
        }

        private async Task<List<RequestStatusChangeDto>> GetHistoryAsync(int id) =>
            (await _client.GetFromJsonAsync<List<RequestStatusChangeDto>>($"/api/requests/{id}/history", JsonOptions))!;

        private async Task<RequestStatsDto> GetStatsAsync(string organization) =>
            (await _client.GetFromJsonAsync<RequestStatsDto>($"/api/requests/stats?organizationName={organization}", JsonOptions))!;
    }
}
