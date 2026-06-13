using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cartons",
                schema: "wms",
                columns: table => new
                {
                    CartonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    CartonCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    GrossWeightKg = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartons", x => x.CartonId);
                });

            migrationBuilder.CreateTable(
                name: "PickLists",
                schema: "wms",
                columns: table => new
                {
                    PickListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickLists", x => x.PickListId);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentOrders",
                schema: "wms",
                columns: table => new
                {
                    ShipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoId = table.Column<int>(type: "int", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RequestedShipDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CarrierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentOrders", x => x.ShipmentId);
                });

            migrationBuilder.CreateTable(
                name: "CartonContents",
                schema: "wms",
                columns: table => new
                {
                    ContentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartonId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartonContents", x => x.ContentId);
                    table.ForeignKey(
                        name: "FK_CartonContents_Cartons_CartonId",
                        column: x => x.CartonId,
                        principalSchema: "wms",
                        principalTable: "Cartons",
                        principalColumn: "CartonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartonContents_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PickListLines",
                schema: "wms",
                columns: table => new
                {
                    PickLineId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickListId = table.Column<int>(type: "int", nullable: false),
                    ShipmentLineId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    RequiredQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    PickedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PickSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickListLines", x => x.PickLineId);
                    table.ForeignKey(
                        name: "FK_PickListLines_PickLists_PickListId",
                        column: x => x.PickListId,
                        principalSchema: "wms",
                        principalTable: "PickLists",
                        principalColumn: "PickListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    PickedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    PackedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_ShipmentLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentLines_ShipmentOrders_ShipmentId",
                        column: x => x.ShipmentId,
                        principalSchema: "wms",
                        principalTable: "ShipmentOrders",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartonContents_CartonId",
                schema: "wms",
                table: "CartonContents",
                column: "CartonId");

            migrationBuilder.CreateIndex(
                name: "IX_CartonContents_ProductCode",
                schema: "wms",
                table: "CartonContents",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_PickListLines_PickListId",
                schema: "wms",
                table: "PickListLines",
                column: "PickListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLines_ProductCode",
                schema: "wms",
                table: "ShipmentLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLines_ShipmentId",
                schema: "wms",
                table: "ShipmentLines",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentOrders_ShipmentCode",
                schema: "wms",
                table: "ShipmentOrders",
                column: "ShipmentCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartonContents",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "PickListLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "ShipmentLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "Cartons",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "PickLists",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "ShipmentOrders",
                schema: "wms");
        }
    }
}
