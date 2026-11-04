using RequestsManagement.DAL.Entities;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.Tests
{
    public static class TestData
    {
        public static string NewOrganization() => $"Org-{Guid.NewGuid():N}";

        public static Request NewRequest(string organization, string title, RequestStatus status) =>
            new(title, organization, status, RequestPriority.Medium, assignedTo: null,
                createdAt: DateTime.UtcNow, updatedAt: DateTime.UtcNow);
    }
}
