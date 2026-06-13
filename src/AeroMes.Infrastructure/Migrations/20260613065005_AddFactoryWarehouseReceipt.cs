using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryWarehouseReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FactoryWarehouseReceipts",
                schema: "wms",
                columns: table => new
                {
                    ReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceiptType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceRequestId = table.Column<int>(type: "int", nullable: true),
                    SupplierOrTransferringUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FactoryWarehouseReceipts", x => x.ReceiptId);
                });

            migrationBuilder.CreateTable(
                name: "FactoryReceiptLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    SpecificationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryReceiptLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_FactoryReceiptLines_FactoryWarehouseReceipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "wms",
                        principalTable: "FactoryWarehouseReceipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactoryReceiptLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactoryReceiptLines_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactoryReceiptLines_DestinationWarehouseId",
                schema: "wms",
                table: "FactoryReceiptLines",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryReceiptLines_ProductCode",
                schema: "wms",
                table: "FactoryReceiptLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryReceiptLines_ReceiptId",
                schema: "wms",
                table: "FactoryReceiptLines",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryWarehouseReceipts_VoucherNumber",
                schema: "wms",
                table: "FactoryWarehouseReceipts",
                column: "VoucherNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactoryReceiptLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "FactoryWarehouseReceipts",
                schema: "wms");
        }
    }
}
