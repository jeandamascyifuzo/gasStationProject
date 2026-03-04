using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_OrganizationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_StationId_TransactionDate",
                table: "Transactions",
                columns: new[] { "OrganizationId", "StationId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_TransactionDate",
                table: "Transactions",
                columns: new[] { "OrganizationId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_UserId_StationId_IsActive",
                table: "Shifts",
                columns: new[] { "UserId", "StationId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_OrganizationId_StationId_TransactionDate",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_OrganizationId_TransactionDate",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_UserId_StationId_IsActive",
                table: "Shifts");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId",
                table: "Transactions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts",
                column: "UserId");
        }
    }
}
