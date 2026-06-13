using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceCostTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "maint");

            migrationBuilder.CreateTable(
                name: "MachineTCO",
                schema: "maint",
                columns: table => new
                {
                    TcoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodYear = table.Column<short>(type: "smallint", nullable: false),
                    PeriodMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    PlannedMaintCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    ActualMaintCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: false),
                    BreakdownCount = table.Column<int>(type: "int", nullable: false),
                    MtbfHours = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: true),
                    MttrHours = table.Column<decimal>(type: "DECIMAL(8,2)", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineTCO", x => x.TcoID);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceOrders",
                schema: "maint",
                columns: table => new
                {
                    MaintOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Normal"),
                    PlannedStartAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    PlannedEndAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ActualStartAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MaintenanceOrders", x => x.MaintOrderID);
                });

            migrationBuilder.CreateTable(
                name: "MaintCostLines",
                schema: "maint",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintOrderID = table.Column<int>(type: "int", nullable: false),
                    CostCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QtyUsed = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: true),
                    UnitCost = table.Column<decimal>(type: "DECIMAL(14,6)", nullable: true),
                    OperatorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LaborHours = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: true),
                    LaborRateSnapshot = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: true),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    InvoiceRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceAmount = table.Column<decimal>(type: "DECIMAL(14,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false),
                    PostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintCostLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_MaintCostLines_MaintenanceOrders_MaintOrderID",
                        column: x => x.MaintOrderID,
                        principalSchema: "maint",
                        principalTable: "MaintenanceOrders",
                        principalColumn: "MaintOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineTCO_MachineCode_PeriodYear_PeriodMonth",
                schema: "maint",
                table: "MachineTCO",
                columns: new[] { "MachineCode", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintCostLines_MaintOrderID",
                schema: "maint",
                table: "MaintCostLines",
                column: "MaintOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_MachineCode",
                schema: "maint",
                table: "MaintenanceOrders",
                column: "MachineCode");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_MaintOrderCode",
                schema: "maint",
                table: "MaintenanceOrders",
                column: "MaintOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_Status",
                schema: "maint",
                table: "MaintenanceOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineTCO",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "MaintCostLines",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "MaintenanceOrders",
                schema: "maint");
        }
    }
}
