using RequestsManagement.DAL.Entities;
using RequestsManagement.DTO.Enums;

namespace RequestsManagement.DAL.Data
{
    public static class RequestDataGenerator
    {
        private static readonly DateTime ReferenceDate = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        private const int HistoryDays = 730;

        private static readonly string[] Organizations =
        {
            "Acme Ltd", "Globex", "Initech", "Umbrella Corp", "Stark Industries",
            "Wayne Enterprises", "Hooli", "Soylent Corp", "Cyberdyne Systems", "Tyrell Corp",
            "Wonka Industries", "Vandelay Industries", "Pied Piper", "Massive Dynamic", "Oscorp",
            "Gringotts", "Monsters Inc", "Dunder Mifflin", "Aperture Science", "Black Mesa",
            "Blue Sun", "Virtucon", "Nakatomi Trading", "Gekko & Co", "Prestige Worldwide",
            "Bluth Company", "Sterling Cooper", "Los Pollos Hermanos", "Krusty Krab", "Planet Express"
        };

        private static readonly string[] Handlers =
        {
            "dana", "yossi", "michal", "avi", "noa", "eitan", "shira", "omer", "tamar", "ron"
        };

        private static readonly string[] Issues =
        {
            "Login failure", "Data sync error", "Slow response", "Missing report", "Permission request",
            "Password reset", "Invoice discrepancy", "Feature request", "Export failed", "Notification not received"
        };

        private static readonly string[] Systems =
        {
            "Billing portal", "CRM", "Mobile app", "HR system", "Inventory", "Customer portal", "Analytics dashboard", "Email service"
        };

        // מייצר פניות מדומות ומציאותיות. אותו seed מייצר תמיד את אותם נתונים
        public static IEnumerable<Request> Generate(int count, int seed = 42)
        {
            var random = new Random(seed);

            for (var i = 0; i < count; i++)
            {
                var createdAt = ReferenceDate.AddMinutes(-random.Next(HistoryDays * 24 * 60));
                var status = PickStatus(random);

                yield return new Request(
                    title: $"{Pick(random, Issues)} - {Pick(random, Systems)}",
                    organizationName: PickOrganization(random),
                    status: status,
                    priority: PickPriority(random),
                    assignedTo: status == RequestStatus.New ? null : Pick(random, Handlers),
                    createdAt: createdAt,
                    updatedAt: PickUpdatedAt(random, createdAt, status));
            }
        }

        private static RequestStatus PickStatus(Random random) => random.Next(100) switch
        {
            < 55 => RequestStatus.Completed,
            < 75 => RequestStatus.InProgress,
            < 90 => RequestStatus.Waiting,
            _ => RequestStatus.New
        };

        private static RequestPriority PickPriority(Random random) => random.Next(100) switch
        {
            < 50 => RequestPriority.Low,
            < 85 => RequestPriority.Medium,
            _ => RequestPriority.High
        };

        private static string PickOrganization(Random random)
        {
            var skewed = Math.Pow(random.NextDouble(), 2);
            return Organizations[(int)(skewed * Organizations.Length)];
        }

        private static DateTime PickUpdatedAt(Random random, DateTime createdAt, RequestStatus status)
        {
            if (status == RequestStatus.New)
            {
                return createdAt;
            }

            var updatedAt = createdAt.AddHours(random.Next(1, 30 * 24));
            return updatedAt > ReferenceDate ? ReferenceDate : updatedAt;
        }

        private static T Pick<T>(Random random, T[] items) => items[random.Next(items.Length)];
    }
}
