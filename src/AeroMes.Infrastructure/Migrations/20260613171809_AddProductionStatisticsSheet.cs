using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionStatisticsSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionStatisticsSheets",
                schema: "prod",
                columns: table => new
                {
                    SheetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SheetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    POID = table.Column<int>(type: "int", nullable: true),
                    MPOId = table.Column<int>(type: "int", nullable: true),
                    ProductionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStatisticsSheets", x => x.SheetId);
                });

            migrationBuilder.CreateTable(
                name: "ByProductLines",
                schema: "prod",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ByProductLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_ByProductLines_ProductionStatisticsSheets_SheetId",
                        column: x => x.SheetId,
                        principalSchema: "prod",
                        principalTable: "ProductionStatisticsSheets",
                        principalColumn: "SheetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialConsumptionLines",
                schema: "prod",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BomStandardQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ActualUsedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VarianceReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialConsumptionLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_MaterialConsumptionLines_ProductionStatisticsSheets_SheetId",
                        column: x => x.SheetId,
                        principalSchema: "prod",
                        principalTable: "ProductionStatisticsSheets",
                        principalColumn: "SheetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOutputLines",
                schema: "prod",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheetId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlannedQty = table.Column<int>(type: "int", nullable: false),
                    QualifiedQty = table.Column<int>(type: "int", nullable: false),
                    DefectiveQty = table.Column<int>(type: "int", nullable: false),
                    DefectCodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOutputLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_ProductionOutputLines_ProductionStatisticsSheets_SheetId",
                        column: x => x.SheetId,
                        principalSchema: "prod",
                        principalTable: "ProductionStatisticsSheets",
                        principalColumn: "SheetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ByProductLines_SheetId",
                schema: "prod",
                table: "ByProductLines",
                column: "SheetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptionLines_SheetId",
                schema: "prod",
                table: "MaterialConsumptionLines",
                column: "SheetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOutputLines_SheetId",
                schema: "prod",
                table: "ProductionOutputLines",
                column: "SheetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatisticsSheets_MPOId",
                schema: "prod",
                table: "ProductionStatisticsSheets",
                column: "MPOId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatisticsSheets_POID",
                schema: "prod",
                table: "ProductionStatisticsSheets",
                column: "POID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatisticsSheets_SheetNumber",
                schema: "prod",
                table: "ProductionStatisticsSheets",
                column: "SheetNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ByProductLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MaterialConsumptionLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "ProductionOutputLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "ProductionStatisticsSheets",
                schema: "prod");
        }
    }
}
