using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWOActualCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WOCostSummaries",
                schema: "cost",
                columns: table => new
                {
                    WOCostID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    StdCostID = table.Column<int>(type: "int", nullable: true),
                    StdTotalCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ActMaterialCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false, defaultValue: 0m),
                    ActLaborCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false, defaultValue: 0m),
                    ActMachineCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false, defaultValue: 0m),
                    ActMaintenanceCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false, defaultValue: 0m),
                    ProducedQtyOK = table.Column<int>(type: "int", nullable: false),
                    ScrapQty = table.Column<int>(type: "int", nullable: false),
                    VarianceDetailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WOCostSummaries", x => x.WOCostID);
                });

            migrationBuilder.CreateTable(
                name: "WOLaborCostLines",
                schema: "cost",
                columns: table => new
                {
                    LineID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    OperatorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LaborGradeID = table.Column<int>(type: "int", nullable: false),
                    ActualHours = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: false),
                    HourlyRateSnapshot = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false),
                    OvertimeMultiplierSnapshot = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false, defaultValue: 1.5m),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WOLaborCostLines", x => x.LineID);
                });

            migrationBuilder.CreateTable(
                name: "WOMachineCostLines",
                schema: "cost",
                columns: table => new
                {
                    LineID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RuntimeHours = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: false),
                    EnergyKWh = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: true),
                    TotalRateSnapshot = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WOMachineCostLines", x => x.LineID);
                });

            migrationBuilder.CreateTable(
                name: "WOMaterialCostLines",
                schema: "cost",
                columns: table => new
                {
                    LineID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    ConsumptionID = table.Column<long>(type: "bigint", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QtyConsumed = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ActualUnitCost = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WOMaterialCostLines", x => x.LineID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WOCostSummaries_WOID",
                schema: "cost",
                table: "WOCostSummaries",
                column: "WOID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WOLaborCostLines_WOID",
                schema: "cost",
                table: "WOLaborCostLines",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_WOMachineCostLines_WOID",
                schema: "cost",
                table: "WOMachineCostLines",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_WOMaterialCostLines_WOID",
                schema: "cost",
                table: "WOMaterialCostLines",
                column: "WOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WOCostSummaries",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "WOLaborCostLines",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "WOMachineCostLines",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "WOMaterialCostLines",
                schema: "cost");
        }
    }
}
