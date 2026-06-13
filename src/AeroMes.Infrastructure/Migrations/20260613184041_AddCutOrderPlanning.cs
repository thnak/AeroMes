using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCutOrderPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bundles",
                schema: "prod",
                columns: table => new
                {
                    BundleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CutOrderID = table.Column<int>(type: "int", nullable: false),
                    SizeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BundleNumber = table.Column<int>(type: "int", nullable: false),
                    PieceCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.BundleID);
                });

            migrationBuilder.CreateTable(
                name: "CutOrders",
                schema: "prod",
                columns: table => new
                {
                    CutOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CutOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FabricProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShadeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MarkerReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarkerEfficiencyPct = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: true),
                    PlyCount = table.Column<int>(type: "int", nullable: false),
                    SpreadLengthMeters = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    FabricWidthCm = table.Column<decimal>(type: "DECIMAL(6,1)", nullable: false),
                    EstimatedFabricMeters = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: true),
                    ActualFabricMeters = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: true),
                    FabricWastePct = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: true, computedColumnSql: "CASE WHEN [ActualFabricMeters] > 0 AND [MarkerEfficiencyPct] > 0 THEN 100.0 - [MarkerEfficiencyPct] ELSE NULL END"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CuttingStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CuttingCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutOrders", x => x.CutOrderID);
                });

            migrationBuilder.CreateTable(
                name: "CutOrderFabricUsage",
                schema: "prod",
                columns: table => new
                {
                    UsageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CutOrderID = table.Column<int>(type: "int", nullable: false),
                    RollID = table.Column<int>(type: "int", nullable: false),
                    MetersUsed = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutOrderFabricUsage", x => x.UsageID);
                    table.ForeignKey(
                        name: "FK_CutOrderFabricUsage_CutOrders_CutOrderID",
                        column: x => x.CutOrderID,
                        principalSchema: "prod",
                        principalTable: "CutOrders",
                        principalColumn: "CutOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CutOrderLines",
                schema: "prod",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CutOrderID = table.Column<int>(type: "int", nullable: false),
                    SizeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QuantityToCut = table.Column<int>(type: "int", nullable: false),
                    QuantityCut = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutOrderLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_CutOrderLines_CutOrders_CutOrderID",
                        column: x => x.CutOrderID,
                        principalSchema: "prod",
                        principalTable: "CutOrders",
                        principalColumn: "CutOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_CutOrderID",
                schema: "prod",
                table: "Bundles",
                column: "CutOrderID");

            migrationBuilder.CreateIndex(
                name: "UQ_CO_Roll",
                schema: "prod",
                table: "CutOrderFabricUsage",
                columns: new[] { "CutOrderID", "RollID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CutOrderLine",
                schema: "prod",
                table: "CutOrderLines",
                columns: new[] { "CutOrderID", "SizeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CutOrder_Style",
                schema: "prod",
                table: "CutOrders",
                columns: new[] { "StyleCode", "ColorCode" });

            migrationBuilder.CreateIndex(
                name: "IX_CutOrder_WOID",
                schema: "prod",
                table: "CutOrders",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_CutOrders_CutOrderCode",
                schema: "prod",
                table: "CutOrders",
                column: "CutOrderCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bundles",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "CutOrderFabricUsage",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "CutOrderLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "CutOrders",
                schema: "prod");
        }
    }
}
