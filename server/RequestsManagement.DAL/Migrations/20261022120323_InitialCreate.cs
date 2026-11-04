using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RequestsManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Requests",
                columns: new[] { "Id", "AssignedTo", "CreatedAt", "OrganizationName", "Priority", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "yossi", new DateTime(2026, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), "Globex", "Medium", "InProgress", "Sample request #1", new DateTime(2026, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "michal", new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Utc), "Initech", "High", "Waiting", "Sample request #2", new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "dana", new DateTime(2026, 1, 4, 9, 0, 0, 0, DateTimeKind.Utc), "Umbrella Corp", "Low", "Completed", "Sample request #3", new DateTime(2026, 1, 4, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "yossi", new DateTime(2026, 1, 5, 9, 0, 0, 0, DateTimeKind.Utc), "Stark Industries", "Medium", "New", "Sample request #4", new DateTime(2026, 1, 5, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "michal", new DateTime(2026, 1, 6, 9, 0, 0, 0, DateTimeKind.Utc), "Acme Ltd", "High", "InProgress", "Sample request #5", new DateTime(2026, 1, 6, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "dana", new DateTime(2026, 1, 7, 9, 0, 0, 0, DateTimeKind.Utc), "Globex", "Low", "Waiting", "Sample request #6", new DateTime(2026, 1, 7, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "yossi", new DateTime(2026, 1, 8, 9, 0, 0, 0, DateTimeKind.Utc), "Initech", "Medium", "Completed", "Sample request #7", new DateTime(2026, 1, 8, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "michal", new DateTime(2026, 1, 9, 9, 0, 0, 0, DateTimeKind.Utc), "Umbrella Corp", "High", "New", "Sample request #8", new DateTime(2026, 1, 9, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "dana", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), "Stark Industries", "Low", "InProgress", "Sample request #9", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "yossi", new DateTime(2026, 1, 11, 9, 0, 0, 0, DateTimeKind.Utc), "Acme Ltd", "Medium", "Waiting", "Sample request #10", new DateTime(2026, 1, 11, 9, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
