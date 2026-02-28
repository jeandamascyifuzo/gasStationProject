using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Customers_CustomerId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_FuelTypes_FuelTypeId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_CustomerId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "UsedLiters",
                table: "Subscriptions",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "PricePerLiter",
                table: "Subscriptions",
                newName: "TopUpAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyLiters",
                table: "Subscriptions",
                newName: "RemainingBalance");

            migrationBuilder.RenameColumn(
                name: "FuelTypeId",
                table: "Subscriptions",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_FuelTypeId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_OrganizationId");

            migrationBuilder.AddColumn<decimal>(
                name: "SubscriptionDeduction",
                table: "Transactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBalance",
                table: "Subscriptions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PINHash",
                table: "Cars",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SubscriptionId",
                table: "Transactions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ActivePerCustomer",
                table: "Subscriptions",
                columns: new[] { "CustomerId", "OrganizationId", "Status" },
                unique: true,
                filter: "[Status] = 'Active' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_PlateNumber",
                table: "Cars",
                column: "PlateNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Customers_CustomerId",
                table: "Subscriptions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Organizations_OrganizationId",
                table: "Subscriptions",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Subscriptions_SubscriptionId",
                table: "Transactions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Customers_CustomerId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Organizations_OrganizationId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Subscriptions_SubscriptionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SubscriptionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_ActivePerCustomer",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Cars_PlateNumber",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "SubscriptionDeduction",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PreviousBalance",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "PINHash",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Subscriptions",
                newName: "UsedLiters");

            migrationBuilder.RenameColumn(
                name: "TopUpAmount",
                table: "Subscriptions",
                newName: "PricePerLiter");

            migrationBuilder.RenameColumn(
                name: "RemainingBalance",
                table: "Subscriptions",
                newName: "MonthlyLiters");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Subscriptions",
                newName: "FuelTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_OrganizationId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_FuelTypeId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CustomerId",
                table: "Subscriptions",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Customers_CustomerId",
                table: "Subscriptions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_FuelTypes_FuelTypeId",
                table: "Subscriptions",
                column: "FuelTypeId",
                principalTable: "FuelTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
