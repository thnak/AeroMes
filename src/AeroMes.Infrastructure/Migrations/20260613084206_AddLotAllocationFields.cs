using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLotAllocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinShelfLifeDaysOnIssue",
                schema: "master",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickingStrategy",
                schema: "master",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "prod",
                table: "InventoryStock",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ManufacturedDate",
                schema: "prod",
                table: "InventoryStock",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                schema: "prod",
                table: "InventoryStock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinShelfLifeDaysOnIssue",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PickingStrategy",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "ManufacturedDate",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                schema: "prod",
                table: "InventoryStock");
        }
    }
}
