using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "settings");

            migrationBuilder.CreateTable(
                name: "SystemOptions",
                schema: "settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderRetrievalPrevention = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseOrderAutoGenerateProductionPlan = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseOrderDefaultAllocationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaterialManagementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaterialAutoGenerateWarehouseDocs = table.Column<bool>(type: "bit", nullable: false),
                    MaterialTrackExcessAndByProducts = table.Column<bool>(type: "bit", nullable: false),
                    MaterialByProductTracking = table.Column<bool>(type: "bit", nullable: false),
                    MaterialBatchAndExpiryManagement = table.Column<bool>(type: "bit", nullable: false),
                    MaterialDimensionTracking = table.Column<bool>(type: "bit", nullable: false),
                    MaterialUnitConversionEditable = table.Column<bool>(type: "bit", nullable: false),
                    MaterialForecastStockWarning = table.Column<bool>(type: "bit", nullable: false),
                    MaterialDefectRateManagement = table.Column<bool>(type: "bit", nullable: false),
                    CapacityMoldToolingManagement = table.Column<bool>(type: "bit", nullable: false),
                    DispatchAutoGenerateSubAssemblyOrders = table.Column<bool>(type: "bit", nullable: false),
                    DispatchAutoStatusTransition = table.Column<bool>(type: "bit", nullable: false),
                    DispatchSequentialWorkflowEnforcement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DispatchAutoGenerateSupplyRequests = table.Column<bool>(type: "bit", nullable: false),
                    ReportingOverageQuantityAlert = table.Column<bool>(type: "bit", nullable: false),
                    ReportingEditLockAfterQcRequest = table.Column<bool>(type: "bit", nullable: false),
                    ReportingAutoGenerateFromCompletedWorkOrders = table.Column<bool>(type: "bit", nullable: false),
                    ReportingPrintLimitEnforcement = table.Column<bool>(type: "bit", nullable: false),
                    HandoffTrackInterStageTransfers = table.Column<bool>(type: "bit", nullable: false),
                    HandoffAutoConfirmation = table.Column<bool>(type: "bit", nullable: false),
                    AcceptanceAutoGenerateFromProductionReports = table.Column<bool>(type: "bit", nullable: false),
                    AcceptanceQuantityCategorization = table.Column<bool>(type: "bit", nullable: false),
                    QcAutoGenerateRequestsAfterReporting = table.Column<bool>(type: "bit", nullable: false),
                    QcTargetSelection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOptions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemOptions",
                schema: "settings");
        }
    }
}
