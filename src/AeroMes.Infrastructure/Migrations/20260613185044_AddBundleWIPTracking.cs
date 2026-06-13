using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBundleWIPTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PieceCount",
                schema: "prod",
                table: "Bundles",
                newName: "Quantity");

            migrationBuilder.RenameIndex(
                name: "IX_Bundles_CutOrderID",
                schema: "prod",
                table: "Bundles",
                newName: "IX_Bundle_CutOrder");

            migrationBuilder.AddColumn<string>(
                name: "OperationCategory",
                schema: "master",
                table: "Operations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SAM_Minutes",
                schema: "master",
                table: "Operations",
                type: "DECIMAL(8,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BundleBarcode",
                schema: "prod",
                table: "Bundles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                schema: "prod",
                table: "Bundles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentOperationCode",
                schema: "prod",
                table: "Bundles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentWorkCenterID",
                schema: "prod",
                table: "Bundles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QtyNGCumulative",
                schema: "prod",
                table: "Bundles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QtyOKCumulative",
                schema: "prod",
                table: "Bundles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StyleCode",
                schema: "prod",
                table: "Bundles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BundleMovements",
                schema: "prod",
                columns: table => new
                {
                    MovementID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BundleID = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QtyOK = table.Column<int>(type: "int", nullable: false),
                    QtyNG = table.Column<int>(type: "int", nullable: false),
                    DefectCodes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SAM_Minutes = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: true),
                    ActualMinsPerPiece = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: true, computedColumnSql: "CASE WHEN [EndTime] IS NOT NULL AND [QtyOK] > 0 THEN CAST(DATEDIFF(SECOND, [StartTime], [EndTime]) AS float) / 60.0 / NULLIF([QtyOK], 0) ELSE NULL END"),
                    EfficiencyPct = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: true, computedColumnSql: "CASE WHEN [SAM_Minutes] > 0 AND [EndTime] IS NOT NULL AND [QtyOK] > 0 THEN [SAM_Minutes] / (CAST(DATEDIFF(SECOND, [StartTime], [EndTime]) AS float) / 60.0 / NULLIF([QtyOK], 0)) * 100.0 ELSE NULL END")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleMovements", x => x.MovementID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bundle_Status",
                schema: "prod",
                table: "Bundles",
                columns: new[] { "Status", "CurrentWorkCenterID" },
                filter: "[Status] NOT IN ('Packed')");

            migrationBuilder.CreateIndex(
                name: "IX_Bundle_Style",
                schema: "prod",
                table: "Bundles",
                columns: new[] { "StyleCode", "ColorCode", "SizeCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_BundleBarcode",
                schema: "prod",
                table: "Bundles",
                column: "BundleBarcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BundleMovement_Bundle",
                schema: "prod",
                table: "BundleMovements",
                column: "BundleID");

            migrationBuilder.CreateIndex(
                name: "IX_BundleMovement_Operator",
                schema: "prod",
                table: "BundleMovements",
                columns: new[] { "OperatorID", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BundleMovement_WC",
                schema: "prod",
                table: "BundleMovements",
                columns: new[] { "WorkCenterID", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BundleMovements",
                schema: "prod");

            migrationBuilder.DropIndex(
                name: "IX_Bundle_Status",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropIndex(
                name: "IX_Bundle_Style",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropIndex(
                name: "IX_Bundles_BundleBarcode",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "OperationCategory",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SAM_Minutes",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "BundleBarcode",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "CurrentOperationCode",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "CurrentWorkCenterID",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "QtyNGCumulative",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "QtyOKCumulative",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.DropColumn(
                name: "StyleCode",
                schema: "prod",
                table: "Bundles");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "prod",
                table: "Bundles",
                newName: "PieceCount");

            migrationBuilder.RenameIndex(
                name: "IX_Bundle_CutOrder",
                schema: "prod",
                table: "Bundles",
                newName: "IX_Bundles_CutOrderID");
        }
    }
}
