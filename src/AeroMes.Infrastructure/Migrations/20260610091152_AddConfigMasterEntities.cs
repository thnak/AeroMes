using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigMasterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertThresholds",
                schema: "master",
                columns: table => new
                {
                    ThresholdId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WarningLevel = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false),
                    CriticalLevel = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false),
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
                    table.PrimaryKey("PK_AlertThresholds", x => x.ThresholdId);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeReasonCodes",
                schema: "master",
                columns: table => new
                {
                    ReasonCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SlaMinutes = table.Column<int>(type: "int", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_DowntimeReasonCodes", x => x.ReasonCode);
                });

            migrationBuilder.CreateTable(
                name: "MachineProductConfigs",
                schema: "master",
                columns: table => new
                {
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoutingStepId = table.Column<int>(type: "int", nullable: true),
                    IdealCycleTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TargetThroughputPerHour = table.Column<int>(type: "int", nullable: false),
                    SetupTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineProductConfigs", x => new { x.MachineCode, x.ProductCode });
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_Machines_MachineCode",
                        column: x => x.MachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_RoutingSteps_RoutingStepId",
                        column: x => x.RoutingStepId,
                        principalSchema: "master",
                        principalTable: "RoutingSteps",
                        principalColumn: "RoutingStepID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTemplates",
                schema: "master",
                columns: table => new
                {
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    ValidDays = table.Column<int>(type: "int", nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ShiftTemplates", x => x.ShiftCode);
                    table.ForeignKey(
                        name: "FK_ShiftTemplates_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderAutoRules",
                schema: "master",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true),
                    AutoStartEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AutoCompleteOnTargetReached = table.Column<bool>(type: "bit", nullable: false),
                    RequireDeleteConfirmToken = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentJobs = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WorkOrderAutoRules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_WorkOrderAutoRules_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ShiftCode",
                schema: "prod",
                table: "Jobs",
                column: "ShiftCode");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeLogs_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                column: "ReasonCode");

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductConfigs_ProductCode",
                schema: "master",
                table: "MachineProductConfigs",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductConfigs_RoutingStepId",
                schema: "master",
                table: "MachineProductConfigs",
                column: "RoutingStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTemplates_WorkCenterId",
                schema: "master",
                table: "ShiftTemplates",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAutoRules_WorkCenterId",
                schema: "master",
                table: "WorkOrderAutoRules",
                column: "WorkCenterId",
                unique: true,
                filter: "[WorkCenterId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_DowntimeLogs_DowntimeReasonCodes_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                column: "ReasonCode",
                principalSchema: "master",
                principalTable: "DowntimeReasonCodes",
                principalColumn: "ReasonCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_ShiftTemplates_ShiftCode",
                schema: "prod",
                table: "Jobs",
                column: "ShiftCode",
                principalSchema: "master",
                principalTable: "ShiftTemplates",
                principalColumn: "ShiftCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DowntimeLogs_DowntimeReasonCodes_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_ShiftTemplates_ShiftCode",
                schema: "prod",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "AlertThresholds",
                schema: "master");

            migrationBuilder.DropTable(
                name: "DowntimeReasonCodes",
                schema: "master");

            migrationBuilder.DropTable(
                name: "MachineProductConfigs",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ShiftTemplates",
                schema: "master");

            migrationBuilder.DropTable(
                name: "WorkOrderAutoRules",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ShiftCode",
                schema: "prod",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_DowntimeLogs_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs");
        }
    }
}
