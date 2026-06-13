using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterAndDetailedProductionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetailedProductionPlans",
                schema: "prod",
                columns: table => new
                {
                    DetailPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MasterPlanId = table.Column<int>(type: "int", nullable: false),
                    OrganizationalUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Granularity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HasProductionOrders = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailedProductionPlans", x => x.DetailPlanId);
                });

            migrationBuilder.CreateTable(
                name: "MasterProductionPlans",
                schema: "prod",
                columns: table => new
                {
                    MasterPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizationalUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Granularity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WorkingHoursPerDay = table.Column<decimal>(type: "DECIMAL(4,2)", nullable: false),
                    WorkingDaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterProductionPlans", x => x.MasterPlanId);
                });

            migrationBuilder.CreateTable(
                name: "DppProductLines",
                schema: "prod",
                columns: table => new
                {
                    DppLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailPlanId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TotalRequiredQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    DailyCapacity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DppProductLines", x => x.DppLineId);
                    table.ForeignKey(
                        name: "FK_DppProductLines_DetailedProductionPlans_DetailPlanId",
                        column: x => x.DetailPlanId,
                        principalSchema: "prod",
                        principalTable: "DetailedProductionPlans",
                        principalColumn: "DetailPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterPlanLines",
                schema: "prod",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterPlanId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QuantityRequired = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    DailyCapacity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    OpeningInventory = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ClosingInventoryForecast = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    DistributionStrategy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPlanLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_MasterPlanLines_MasterProductionPlans_MasterPlanId",
                        column: x => x.MasterPlanId,
                        principalSchema: "prod",
                        principalTable: "MasterProductionPlans",
                        principalColumn: "MasterPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DppSlots",
                schema: "prod",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DppLineId = table.Column<int>(type: "int", nullable: false),
                    SlotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AllocatedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DppSlots", x => x.SlotId);
                    table.ForeignKey(
                        name: "FK_DppSlots_DppProductLines_DppLineId",
                        column: x => x.DppLineId,
                        principalSchema: "prod",
                        principalTable: "DppProductLines",
                        principalColumn: "DppLineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailedProductionPlans_MasterPlanId",
                schema: "prod",
                table: "DetailedProductionPlans",
                column: "MasterPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedProductionPlans_PlanNumber",
                schema: "prod",
                table: "DetailedProductionPlans",
                column: "PlanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DppProductLines_DetailPlanId",
                schema: "prod",
                table: "DppProductLines",
                column: "DetailPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DppSlots_DppLineId_SlotDate",
                schema: "prod",
                table: "DppSlots",
                columns: new[] { "DppLineId", "SlotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlanLines_MasterPlanId",
                schema: "prod",
                table: "MasterPlanLines",
                column: "MasterPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterProductionPlans_PlanNumber",
                schema: "prod",
                table: "MasterProductionPlans",
                column: "PlanNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DppSlots",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MasterPlanLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "DppProductLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MasterProductionPlans",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "DetailedProductionPlans",
                schema: "prod");
        }
    }
}
