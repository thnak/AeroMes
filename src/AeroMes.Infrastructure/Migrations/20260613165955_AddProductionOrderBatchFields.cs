using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOrderBatchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                schema: "integration",
                table: "ProductionOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "integration",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "integration",
                table: "ProductionOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Priority",
                schema: "integration",
                table: "ProductionOrders",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)5);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProductionDeadline",
                schema: "integration",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                schema: "integration",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "integration",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "integration",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "integration",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ProductionDeadline",
                schema: "integration",
                table: "ProductionOrders");
        }
    }
}
