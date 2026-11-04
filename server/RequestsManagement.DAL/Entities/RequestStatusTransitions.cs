using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DAL.Entities
{
    public static class RequestStatusTransitions
    {
        private static readonly Dictionary<RequestStatus, RequestStatus[]> Allowed = new()
        {
            [RequestStatus.New] = new[] { RequestStatus.InProgress },
            [RequestStatus.InProgress] = new[] { RequestStatus.Waiting, RequestStatus.Completed },
            [RequestStatus.Waiting] = new[] { RequestStatus.InProgress, RequestStatus.Completed },
            [RequestStatus.Completed] = new[] { RequestStatus.InProgress }
        };

        public static IReadOnlyList<RequestStatus> AllowedFrom(RequestStatus from) => Allowed[from];

        // בודק אם המעבר בין שני הסטטוסים מותר לפי כללי המעברים
        public static bool IsAllowed(RequestStatus from, RequestStatus to) => Allowed[from].Contains(to);
    }
}
