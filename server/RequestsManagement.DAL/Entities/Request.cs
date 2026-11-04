using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DAL.Entities
{
    public class Request
    {
        public Request(string title, string organizationName, RequestStatus status, RequestPriority priority,
            string? assignedTo, DateTime createdAt, DateTime updatedAt)
        {
            Title = title;
            OrganizationName = organizationName;
            Status = status;
            Priority = priority;
            AssignedTo = assignedTo;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        private Request()
        {
        }

        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string OrganizationName { get; set; } = null!;

        public RequestStatus Status { get; private set; }

        public RequestPriority Priority { get; set; }

        public string? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; private set; }

        public byte[] RowVersion { get; set; } = null!;

        // משנה סטטוס אם המעבר מותר, ומעדכן את UpdatedAt. מחזיר false אם המעבר אסור
        public bool TryChangeStatus(RequestStatus newStatus, DateTime now)
        {
            if (!RequestStatusTransitions.IsAllowed(Status, newStatus))
            {
                return false;
            }

            Status = newStatus;
            UpdatedAt = now;
            return true;
        }
    }
}
