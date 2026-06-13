using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricRollManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricConsumptionLog",
                schema: "prod",
                columns: table => new
                {
                    ConsumptionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RollID = table.Column<int>(type: "int", nullable: false),
                    CutOrderID = table.Column<int>(type: "int", nullable: false),
                    MetersConsumed = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    RemainingAfter = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RecordedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricConsumptionLog", x => x.ConsumptionID);
                });

            migrationBuilder.CreateTable(
                name: "FabricRolls",
                schema: "master",
                columns: table => new
                {
                    RollID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RollBarcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FabricProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShadeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GrossLengthMeters = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    GrossWeightKg = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    RemainingLengthMeters = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    RemainingWeightKg = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false, computedColumnSql: "[GrossWeightKg] * [RemainingLengthMeters] / [GrossLengthMeters]"),
                    FabricWidthCm = table.Column<decimal>(type: "DECIMAL(6,1)", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricRolls", x => x.RollID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricLog_CutOrder",
                schema: "prod",
                table: "FabricConsumptionLog",
                column: "CutOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_FabricLog_Roll",
                schema: "prod",
                table: "FabricConsumptionLog",
                column: "RollID");

            migrationBuilder.CreateIndex(
                name: "IX_FabricRoll_Barcode",
                schema: "master",
                table: "FabricRolls",
                column: "RollBarcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricRoll_Lot",
                schema: "master",
                table: "FabricRolls",
                column: "LotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FabricRoll_Product_Shade",
                schema: "master",
                table: "FabricRolls",
                columns: new[] { "FabricProductCode", "ShadeCode" },
                filter: "[Status] = 'Available'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricConsumptionLog",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "FabricRolls",
                schema: "master");
        }
    }
}
