using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEBMCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EBMCategoryId",
                table: "OrganizationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBMCategoryId",
                table: "OrganizationSettings");
        }
    }
}
