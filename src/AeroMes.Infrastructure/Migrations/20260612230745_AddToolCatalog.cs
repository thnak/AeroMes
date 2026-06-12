using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddToolCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tools",
                schema: "master",
                columns: table => new
                {
                    ToolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ToolType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MaxUsageCount = table.Column<int>(type: "int", nullable: true),
                    CurrentUsageCount = table.Column<int>(type: "int", nullable: false),
                    UsageCountAtLastPm = table.Column<int>(type: "int", nullable: false),
                    PmIntervalCount = table.Column<int>(type: "int", nullable: true),
                    RequiresCalibration = table.Column<bool>(type: "bit", nullable: false),
                    CalibrationIntervalDays = table.Column<int>(type: "int", nullable: true),
                    LastCalibratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextCalibrationDue = table.Column<DateOnly>(type: "date", nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentWorkCenterId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Tools", x => x.ToolId);
                    table.ForeignKey(
                        name: "FK_Tools_WorkCenters_CurrentWorkCenterId",
                        column: x => x.CurrentWorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolCheckouts",
                schema: "master",
                columns: table => new
                {
                    CheckoutId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: false),
                    CheckedOutBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CheckedOutAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedReturnAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionOnReturn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCheckouts", x => x.CheckoutId);
                    table.ForeignKey(
                        name: "FK_ToolCheckouts_Tools_ToolId",
                        column: x => x.ToolId,
                        principalSchema: "master",
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolCheckouts_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolMaintenanceLogs",
                schema: "master",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsageAtEvent = table.Column<int>(type: "int", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cost = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: true),
                    NextDueCount = table.Column<int>(type: "int", nullable: true),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolMaintenanceLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_ToolMaintenanceLogs_Tools_ToolId",
                        column: x => x.ToolId,
                        principalSchema: "master",
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolOperationMappings",
                schema: "master",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    UsageCountPerOp = table.Column<decimal>(type: "NUMERIC(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolOperationMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_ToolOperationMappings_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalSchema: "master",
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolOperationMappings_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolOperationMappings_Tools_ToolId",
                        column: x => x.ToolId,
                        principalSchema: "master",
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolCheckouts_ToolId_ReturnedAt",
                schema: "master",
                table: "ToolCheckouts",
                columns: new[] { "ToolId", "ReturnedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ToolCheckouts_WorkCenterId",
                schema: "master",
                table: "ToolCheckouts",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolMaintenanceLogs_ToolId",
                schema: "master",
                table: "ToolMaintenanceLogs",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolOperationMappings_OperationCode",
                schema: "master",
                table: "ToolOperationMappings",
                column: "OperationCode");

            migrationBuilder.CreateIndex(
                name: "IX_ToolOperationMappings_ProductCode",
                schema: "master",
                table: "ToolOperationMappings",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ToolOperationMappings_ToolId_OperationCode_ProductCode",
                schema: "master",
                table: "ToolOperationMappings",
                columns: new[] { "ToolId", "OperationCode", "ProductCode" },
                unique: true,
                filter: "[ProductCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_CurrentWorkCenterId",
                schema: "master",
                table: "Tools",
                column: "CurrentWorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_ToolCode",
                schema: "master",
                table: "Tools",
                column: "ToolCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolCheckouts",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ToolMaintenanceLogs",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ToolOperationMappings",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Tools",
                schema: "master");
        }
    }
}
