using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryWarehouseExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FactoryWarehouseExports",
                schema: "wms",
                columns: table => new
                {
                    ExportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExportType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceRequestId = table.Column<int>(type: "int", nullable: true),
                    ReceiverOrReceivingUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_FactoryWarehouseExports", x => x.ExportId);
                });

            migrationBuilder.CreateTable(
                name: "FactoryExportLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false),
                    SpecificationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryExportLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_FactoryExportLines_FactoryWarehouseExports_ExportId",
                        column: x => x.ExportId,
                        principalSchema: "wms",
                        principalTable: "FactoryWarehouseExports",
                        principalColumn: "ExportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactoryExportLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactoryExportLines_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactoryExportLines_ExportId",
                schema: "wms",
                table: "FactoryExportLines",
                column: "ExportId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryExportLines_ProductCode",
                schema: "wms",
                table: "FactoryExportLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryExportLines_SourceWarehouseId",
                schema: "wms",
                table: "FactoryExportLines",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryWarehouseExports_VoucherNumber",
                schema: "wms",
                table: "FactoryWarehouseExports",
                column: "VoucherNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactoryExportLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "FactoryWarehouseExports",
                schema: "wms");
        }
    }
}
