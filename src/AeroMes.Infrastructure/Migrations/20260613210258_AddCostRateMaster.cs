using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostRateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnergyTariffs",
                schema: "cost",
                columns: table => new
                {
                    TariffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TariffName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TariffType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeakRateKWh = table.Column<decimal>(type: "DECIMAL(12,6)", nullable: false),
                    OffPeakRateKWh = table.Column<decimal>(type: "DECIMAL(12,6)", nullable: true),
                    PeakHourStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    PeakHourEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyTariffs", x => x.TariffID);
                });

            migrationBuilder.CreateTable(
                name: "ItemCostHistory",
                schema: "cost",
                columns: table => new
                {
                    CostID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CostType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitCost = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    CostUoM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCostHistory", x => x.CostID);
                });

            migrationBuilder.CreateTable(
                name: "LaborGrades",
                schema: "cost",
                columns: table => new
                {
                    LaborGradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GradeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    OvertimeMultiplier = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false, defaultValue: 1.5m),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborGrades", x => x.LaborGradeID);
                });

            migrationBuilder.CreateTable(
                name: "MachineCostRates",
                schema: "cost",
                columns: table => new
                {
                    RateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RateType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RatePerHour = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineCostRates", x => x.RateID);
                });

            migrationBuilder.CreateTable(
                name: "MachineEnergyProfiles",
                schema: "cost",
                columns: table => new
                {
                    ProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NominalKW = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    LoadFactor = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false, defaultValue: 0.75m),
                    TariffID = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineEnergyProfiles", x => x.ProfileID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCostHistory_ProductCode",
                schema: "cost",
                table: "ItemCostHistory",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_LaborGrades_GradeCode",
                schema: "cost",
                table: "LaborGrades",
                column: "GradeCode");

            migrationBuilder.CreateIndex(
                name: "IX_LaborGrades_GradeCode_EffectiveFrom",
                schema: "cost",
                table: "LaborGrades",
                columns: new[] { "GradeCode", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineCostRates_MachineCode",
                schema: "cost",
                table: "MachineCostRates",
                column: "MachineCode");

            migrationBuilder.CreateIndex(
                name: "IX_MachineEnergyProfiles_MachineCode",
                schema: "cost",
                table: "MachineEnergyProfiles",
                column: "MachineCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergyTariffs",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "ItemCostHistory",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "LaborGrades",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "MachineCostRates",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "MachineEnergyProfiles",
                schema: "cost");
        }
    }
}
