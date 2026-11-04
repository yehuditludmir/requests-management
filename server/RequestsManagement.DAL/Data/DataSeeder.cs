using System.Data;
using System.Diagnostics;
using RequestsManagement.DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RequestsManagement.DAL.Data
{
    public class DataSeeder
    {
        public const int DefaultCount = 100_000;
        private const int BatchSize = 10_000;

        private readonly AppDbContext _db;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(AppDbContext db, ILogger<DataSeeder> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ממלא את המסד ב-100,000 פניות. עם reset, מוחק ויוצר את המסד מחדש. בלי reset, מדלג אם כבר יש נתונים
        public async Task SeedAsync(bool reset, CancellationToken cancellationToken = default)
        {
            if (reset)
            {
                await _db.Database.EnsureDeletedAsync(cancellationToken);
                await _db.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database recreated");
            }
            else if (await _db.Requests.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Requests table already has data - skipping seed (use --reset to reseed)");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            await SeedWithBulkCopyAsync(RequestDataGenerator.Generate(DefaultCount), cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("Seeded {Count:N0} requests in {ElapsedMs:N0} ms", DefaultCount, stopwatch.ElapsedMilliseconds);
        }

        // טוען את הפניות למסד בקבוצות, בעזרת SqlBulkCopy
        private async Task SeedWithBulkCopyAsync(IEnumerable<Request> requests, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_db.Database.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            using var bulkCopy = new SqlBulkCopy(connection) { DestinationTableName = "Requests" };
            var table = CreateRequestsTable();

            foreach (DataColumn column in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            foreach (var batch in requests.Chunk(BatchSize))
            {
                table.Clear();

                foreach (var r in batch)
                {
                    table.Rows.Add(
                        r.Title,
                        r.OrganizationName,
                        (int)r.Status,
                        (int)r.Priority,
                        (object?)r.AssignedTo ?? DBNull.Value,
                        r.CreatedAt,
                        r.UpdatedAt);
                }

                await bulkCopy.WriteToServerAsync(table, cancellationToken);
            }
        }

        private static DataTable CreateRequestsTable()
        {
            var table = new DataTable();
            table.Columns.Add(nameof(Request.Title), typeof(string));
            table.Columns.Add(nameof(Request.OrganizationName), typeof(string));
            table.Columns.Add(nameof(Request.Status), typeof(int));
            table.Columns.Add(nameof(Request.Priority), typeof(int));
            table.Columns.Add(nameof(Request.AssignedTo), typeof(string));
            table.Columns.Add(nameof(Request.CreatedAt), typeof(DateTime));
            table.Columns.Add(nameof(Request.UpdatedAt), typeof(DateTime));
            return table;
        }
    }
}
