using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderAndGrn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "wms",
                columns: table => new
                {
                    PoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.PoId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierCode",
                        column: x => x.SupplierCode,
                        principalSchema: "master",
                        principalTable: "Suppliers",
                        principalColumn: "SupplierCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "wms",
                columns: table => new
                {
                    MovementId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_StockMovements_Bins_BinId",
                        column: x => x.BinId,
                        principalSchema: "wms",
                        principalTable: "Bins",
                        principalColumn: "BinId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_StorageLocations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "master",
                        principalTable: "StorageLocations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptNotes",
                schema: "wms",
                columns: table => new
                {
                    GrnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: true),
                    StorageLocationId = table.Column<int>(type: "int", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryNoteRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptNotes", x => x.GrnId);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_PurchaseOrders_PoId",
                        column: x => x.PoId,
                        principalSchema: "wms",
                        principalTable: "PurchaseOrders",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "master",
                        principalTable: "StorageLocations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                schema: "wms",
                columns: table => new
                {
                    PoLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    ExpectedLotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.PoLineId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PoId",
                        column: x => x.PoId,
                        principalSchema: "wms",
                        principalTable: "PurchaseOrders",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrnLines",
                schema: "wms",
                columns: table => new
                {
                    GrnLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnId = table.Column<int>(type: "int", nullable: false),
                    PoLineId = table.Column<int>(type: "int", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ManufacturedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GrossWeightKg = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    QcStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DestinationBinId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrnLines", x => x.GrnLineId);
                    table.ForeignKey(
                        name: "FK_GrnLines_Bins_DestinationBinId",
                        column: x => x.DestinationBinId,
                        principalSchema: "wms",
                        principalTable: "Bins",
                        principalColumn: "BinId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GrnLines_GoodsReceiptNotes_GrnId",
                        column: x => x.GrnId,
                        principalSchema: "wms",
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "GrnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrnLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrnLines_PurchaseOrderLines_PoLineId",
                        column: x => x.PoLineId,
                        principalSchema: "wms",
                        principalTable: "PurchaseOrderLines",
                        principalColumn: "PoLineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_GrnCode",
                schema: "wms",
                table: "GoodsReceiptNotes",
                column: "GrnCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_PoId",
                schema: "wms",
                table: "GoodsReceiptNotes",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_StorageLocationId",
                schema: "wms",
                table: "GoodsReceiptNotes",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnLines_DestinationBinId",
                schema: "wms",
                table: "GrnLines",
                column: "DestinationBinId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnLines_GrnId",
                schema: "wms",
                table: "GrnLines",
                column: "GrnId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnLines_PoLineId",
                schema: "wms",
                table: "GrnLines",
                column: "PoLineId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnLines_ProductCode",
                schema: "wms",
                table: "GrnLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PoId",
                schema: "wms",
                table: "PurchaseOrderLines",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_ProductCode",
                schema: "wms",
                table: "PurchaseOrderLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PoCode",
                schema: "wms",
                table: "PurchaseOrders",
                column: "PoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierCode",
                schema: "wms",
                table: "PurchaseOrders",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BinId",
                schema: "wms",
                table: "StockMovements",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_LocationId",
                schema: "wms",
                table: "StockMovements",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductCode_LotNumber",
                schema: "wms",
                table: "StockMovements",
                columns: new[] { "ProductCode", "LotNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_Reference",
                schema: "wms",
                table: "StockMovements",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrnLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "StockMovements",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "GoodsReceiptNotes",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "wms");
        }
    }
}
