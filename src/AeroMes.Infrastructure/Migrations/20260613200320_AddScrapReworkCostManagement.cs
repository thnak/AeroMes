using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapReworkCostManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cost");

            migrationBuilder.CreateTable(
                name: "QualityCostSummaries",
                schema: "cost",
                columns: table => new
                {
                    SummaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodYear = table.Column<short>(type: "smallint", nullable: false),
                    PeriodMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WorkCenterID = table.Column<int>(type: "int", nullable: true),
                    PreventionCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    AppraisalCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    InternalScrapCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    ReworkCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    CustomerReturnCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    WarrantyCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    TotalQualityCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    TotalProductionValue = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: true),
                    CopqPct = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityCostSummaries", x => x.SummaryID);
                });

            migrationBuilder.CreateTable(
                name: "ReworkOrders",
                schema: "cost",
                columns: table => new
                {
                    ReworkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReworkCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceWOID = table.Column<int>(type: "int", nullable: false),
                    ScrapTxID = table.Column<long>(type: "bigint", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReworkQty = table.Column<int>(type: "int", nullable: false),
                    ReworkStepFromId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    ActMaterialCost = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    ActLaborCost = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    ActMachineCost = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    ActTotalReworkCost = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
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
                    table.PrimaryKey("PK_ReworkOrders", x => x.ReworkID);
                });

            migrationBuilder.CreateTable(
                name: "ScrapTransactions",
                schema: "cost",
                columns: table => new
                {
                    ScrapTxID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    LogID = table.Column<long>(type: "bigint", nullable: true),
                    DefectCodeId = table.Column<int>(type: "int", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScrapQty = table.Column<int>(type: "int", nullable: false),
                    MaterialCostPerUnit = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: false),
                    LaborCostSunk = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: false),
                    MachineCostSunk = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: false),
                    TotalScrapCost = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: false),
                    DisposalMethod = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false, defaultValue: "Scrap"),
                    ScrapLocationId = table.Column<int>(type: "int", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScrapAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapTransactions", x => x.ScrapTxID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityCostSummaries_PeriodYear_PeriodMonth_ProductCode_WorkCenterID",
                schema: "cost",
                table: "QualityCostSummaries",
                columns: new[] { "PeriodYear", "PeriodMonth", "ProductCode", "WorkCenterID" },
                unique: true,
                filter: "[ProductCode] IS NOT NULL AND [WorkCenterID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReworkOrders_ReworkCode",
                schema: "cost",
                table: "ReworkOrders",
                column: "ReworkCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTransactions_DefectCodeId",
                schema: "cost",
                table: "ScrapTransactions",
                column: "DefectCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTransactions_ScrapAt",
                schema: "cost",
                table: "ScrapTransactions",
                column: "ScrapAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTransactions_WOID",
                schema: "cost",
                table: "ScrapTransactions",
                column: "WOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityCostSummaries",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "ReworkOrders",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "ScrapTransactions",
                schema: "cost");
        }
    }
}
