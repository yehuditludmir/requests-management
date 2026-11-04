using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestsManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests",
                columns: new[] { "AssignedTo", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests",
                columns: new[] { "OrganizationName", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests");
        }
    }
}
