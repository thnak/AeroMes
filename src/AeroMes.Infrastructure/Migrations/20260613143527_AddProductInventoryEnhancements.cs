using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInventoryEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomAttributes",
                schema: "master",
                table: "Products",
                type: "NVARCHAR(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductClass",
                schema: "master",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryUnit",
                schema: "master",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryUnitConversionFactor",
                schema: "master",
                table: "Products",
                type: "NUMERIC(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingMethod",
                schema: "master",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryQty",
                schema: "prod",
                table: "ProductionLogs",
                type: "NUMERIC(18,4)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "ProcessParameters",
                schema: "prod",
                table: "ProductionLogs",
                type: "NVARCHAR(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryQty",
                schema: "prod",
                table: "ProductionLogs",
                type: "NUMERIC(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                schema: "prod",
                table: "ProductionLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedQty",
                schema: "prod",
                table: "InventoryStock",
                type: "NUMERIC(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryQty",
                schema: "prod",
                table: "InventoryStock",
                type: "NUMERIC(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStock_ExpiryDate",
                schema: "prod",
                table: "InventoryStock",
                column: "ExpiryDate",
                filter: "[ExpiryDate] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryStock_ExpiryDate",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "CustomAttributes",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductClass",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecondaryUnit",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecondaryUnitConversionFactor",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackingMethod",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PrimaryQty",
                schema: "prod",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "ProcessParameters",
                schema: "prod",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "SecondaryQty",
                schema: "prod",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                schema: "prod",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "ReservedQty",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "SecondaryQty",
                schema: "prod",
                table: "InventoryStock");
        }
    }
}
