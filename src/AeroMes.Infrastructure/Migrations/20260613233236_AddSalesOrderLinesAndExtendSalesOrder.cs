using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderLinesAndExtendSalesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                schema: "integration",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedBy",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "integration",
                table: "SalesOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "integration",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacilityCode",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "integration",
                table: "SalesOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncSource",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "integration",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesOrderLines",
                schema: "integration",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SOID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_SalesOrderLines_SalesOrders_SOID",
                        column: x => x.SOID,
                        principalSchema: "integration",
                        principalTable: "SalesOrders",
                        principalColumn: "SOID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderLines_SOID",
                schema: "integration",
                table: "SalesOrderLines",
                column: "SOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderLines",
                schema: "integration");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "FacilityCode",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SyncSource",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
