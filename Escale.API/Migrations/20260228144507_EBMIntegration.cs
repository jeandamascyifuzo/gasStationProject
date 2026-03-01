using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class EBMIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EBMErrorMessage",
                table: "Transactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EBMRetryCount",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EBMSentAt",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMBranchId",
                table: "OrganizationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMBusinessId",
                table: "OrganizationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMCompanyAddress",
                table: "OrganizationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMCompanyName",
                table: "OrganizationSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMCompanyPhone",
                table: "OrganizationSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMCompanyTIN",
                table: "OrganizationSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMStockId",
                table: "InventoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EBMSupplyPrice",
                table: "FuelTypes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBMVariantId",
                table: "FuelTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBMErrorMessage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EBMRetryCount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EBMSentAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EBMBranchId",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMBusinessId",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMCompanyAddress",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMCompanyName",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMCompanyPhone",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMCompanyTIN",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "EBMStockId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "EBMSupplyPrice",
                table: "FuelTypes");

            migrationBuilder.DropColumn(
                name: "EBMVariantId",
                table: "FuelTypes");
        }
    }
}
