using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escale.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEBMProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EBMProductId",
                table: "FuelTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBMProductId",
                table: "FuelTypes");
        }
    }
}
