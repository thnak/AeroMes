using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBeginningInventoryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeginningInventoryEntries",
                schema: "wms",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<DateOnly>(type: "date", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BeginningQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningInventoryEntries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_BeginningInventoryEntries_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeginningInventoryEntries_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeginningInventoryEntries_Period_WarehouseId_ProductCode_LotNumber",
                schema: "wms",
                table: "BeginningInventoryEntries",
                columns: new[] { "Period", "WarehouseId", "ProductCode", "LotNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_BeginningInventoryEntries_ProductCode",
                schema: "wms",
                table: "BeginningInventoryEntries",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningInventoryEntries_WarehouseId",
                schema: "wms",
                table: "BeginningInventoryEntries",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeginningInventoryEntries",
                schema: "wms");
        }
    }
}
