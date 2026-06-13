using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnergyMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "energy");

            migrationBuilder.CreateTable(
                name: "EnergyTargets",
                schema: "energy",
                columns: table => new
                {
                    TargetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetKWhPerUnit = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false),
                    TargetKWhPerShift = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyTargets", x => x.TargetID);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                schema: "energy",
                columns: table => new
                {
                    ReadingID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterID = table.Column<int>(type: "int", nullable: false),
                    ReadingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReadingValue = table.Column<decimal>(type: "DECIMAL(14,3)", nullable: false),
                    ReadingAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnteredBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.ReadingID);
                });

            migrationBuilder.CreateTable(
                name: "Meters",
                schema: "energy",
                columns: table => new
                {
                    MeterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UtilityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WorkCenterID = table.Column<int>(type: "int", nullable: true),
                    IsSubMeter = table.Column<bool>(type: "bit", nullable: false),
                    ParentMeterID = table.Column<int>(type: "int", nullable: true),
                    TariffID = table.Column<int>(type: "int", nullable: true),
                    OpcUaNodeId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Meters", x => x.MeterID);
                });

            migrationBuilder.CreateTable(
                name: "ShiftConsumptions",
                schema: "energy",
                columns: table => new
                {
                    ConsumptionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterID = table.Column<int>(type: "int", nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartReadingID = table.Column<long>(type: "bigint", nullable: false),
                    EndReadingID = table.Column<long>(type: "bigint", nullable: true),
                    ConsumedUnits = table.Column<decimal>(type: "DECIMAL(14,3)", nullable: true),
                    EnergyCost = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true),
                    QtyProduced = table.Column<int>(type: "int", nullable: true),
                    EnergyIntensity = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: true),
                    WOID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftConsumptions", x => x.ConsumptionID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_MeterID_ReadingAt",
                schema: "energy",
                table: "MeterReadings",
                columns: new[] { "MeterID", "ReadingAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Meters_MeterCode",
                schema: "energy",
                table: "Meters",
                column: "MeterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftConsumptions_MeterID_ShiftDate",
                schema: "energy",
                table: "ShiftConsumptions",
                columns: new[] { "MeterID", "ShiftDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergyTargets",
                schema: "energy");

            migrationBuilder.DropTable(
                name: "MeterReadings",
                schema: "energy");

            migrationBuilder.DropTable(
                name: "Meters",
                schema: "energy");

            migrationBuilder.DropTable(
                name: "ShiftConsumptions",
                schema: "energy");
        }
    }
}
