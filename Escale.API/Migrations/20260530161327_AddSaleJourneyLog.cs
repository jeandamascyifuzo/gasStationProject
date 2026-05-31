using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleJourneyLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleJourneyLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Liters = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PricePerLiter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsSubscriptionSale = table.Column<bool>(type: "bit", nullable: false),
                    FuelTypeLookupMs = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionCheckMs = table.Column<long>(type: "bigint", nullable: false),
                    EBMSubmissionMs = table.Column<long>(type: "bigint", nullable: false),
                    DBSaveMs = table.Column<long>(type: "bigint", nullable: false),
                    TotalDurationMs = table.Column<long>(type: "bigint", nullable: false),
                    EBMRequired = table.Column<bool>(type: "bit", nullable: false),
                    EBMSuccess = table.Column<bool>(type: "bit", nullable: false),
                    EBMError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionRequired = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionValid = table.Column<bool>(type: "bit", nullable: true),
                    SubscriptionFailReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    FailureStep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleJourneyLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleJourneyLogs_CashierId",
                table: "SaleJourneyLogs",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleJourneyLogs_OrganizationId",
                table: "SaleJourneyLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleJourneyLogs_Timestamp",
                table: "SaleJourneyLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleJourneyLogs");
        }
    }
}
