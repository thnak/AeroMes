using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductCategoryGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "master",
                table: "ProductCategories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "master",
                table: "ProductCategories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardProductionTime",
                schema: "master",
                table: "ProductCategories",
                type: "NUMERIC(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                schema: "master",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "master",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "StandardProductionTime",
                schema: "master",
                table: "ProductCategories");
        }
    }
}
