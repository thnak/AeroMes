using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialSupplyRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialSupplyRequests",
                schema: "wms",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequesterUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_MaterialSupplyRequests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "MaterialSupplyRequestLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSupplyRequestLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_MaterialSupplyRequestLines_MaterialSupplyRequests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "wms",
                        principalTable: "MaterialSupplyRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialSupplyRequestLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialSupplyRequestLines_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSupplyRequestLines_ProductCode",
                schema: "wms",
                table: "MaterialSupplyRequestLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSupplyRequestLines_RequestId",
                schema: "wms",
                table: "MaterialSupplyRequestLines",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSupplyRequestLines_WarehouseId",
                schema: "wms",
                table: "MaterialSupplyRequestLines",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSupplyRequests_VoucherNumber",
                schema: "wms",
                table: "MaterialSupplyRequests",
                column: "VoucherNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialSupplyRequestLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "MaterialSupplyRequests",
                schema: "wms");
        }
    }
}
