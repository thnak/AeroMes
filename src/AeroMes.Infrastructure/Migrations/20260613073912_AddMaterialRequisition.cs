using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialRequisition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialRequisitions",
                schema: "wms",
                columns: table => new
                {
                    RequisitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: true),
                    RequesterUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MaterialRequisitions", x => x.RequisitionId);
                    table.ForeignKey(
                        name: "FK_MaterialRequisitions_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalSchema: "integration",
                        principalTable: "ProductionOrders",
                        principalColumn: "POID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialRequisitionLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ActualIssuedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRequisitionLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_MaterialRequisitionLines_MaterialRequisitions_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "wms",
                        principalTable: "MaterialRequisitions",
                        principalColumn: "RequisitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialRequisitionLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialRequisitionLines_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequisitionLines_ProductCode",
                schema: "wms",
                table: "MaterialRequisitionLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequisitionLines_RequisitionId",
                schema: "wms",
                table: "MaterialRequisitionLines",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequisitionLines_WarehouseId",
                schema: "wms",
                table: "MaterialRequisitionLines",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequisitions_ProductionOrderId",
                schema: "wms",
                table: "MaterialRequisitions",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequisitions_RequisitionNumber",
                schema: "wms",
                table: "MaterialRequisitions",
                column: "RequisitionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialRequisitionLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "MaterialRequisitions",
                schema: "wms");
        }
    }
}
