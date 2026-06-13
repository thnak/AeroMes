using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionPlanByOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionPlansByOrder",
                schema: "prod",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    AllocationMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ProductionPlansByOrder", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPlanOrderLines",
                schema: "prod",
                columns: table => new
                {
                    PlanLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlannedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlanOrderLines", x => x.PlanLineId);
                    table.ForeignKey(
                        name: "FK_ProductionPlanOrderLines_ProductionPlansByOrder_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "prod",
                        principalTable: "ProductionPlansByOrder",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanOrderLines_PlanId",
                schema: "prod",
                table: "ProductionPlanOrderLines",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlansByOrder_PlanCode",
                schema: "prod",
                table: "ProductionPlansByOrder",
                column: "PlanCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPlanOrderLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "ProductionPlansByOrder",
                schema: "prod");
        }
    }
}
