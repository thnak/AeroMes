using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardCostRollup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StandardCostHeaders",
                schema: "cost",
                columns: table => new
                {
                    StdCostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BomHeaderId = table.Column<int>(type: "int", nullable: true),
                    RoutingId = table.Column<int>(type: "int", nullable: true),
                    CostVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalMaterialCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    TotalLaborCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    TotalMachineCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    TotalOverheadCost = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_StandardCostHeaders", x => x.StdCostId);
                });

            migrationBuilder.CreateTable(
                name: "StandardCostMaterialLines",
                schema: "cost",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StdCostId = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ScrapFactor = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardCostMaterialLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_StandardCostMaterialLines_StandardCostHeaders_StdCostId",
                        column: x => x.StdCostId,
                        principalSchema: "cost",
                        principalTable: "StandardCostHeaders",
                        principalColumn: "StdCostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StandardCostRoutingLines",
                schema: "cost",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StdCostId = table.Column<int>(type: "int", nullable: false),
                    RoutingStepId = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CycleTimeSec = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    LaborRateSnapshot = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    MachineRateSnapshot = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardCostRoutingLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_StandardCostRoutingLines_StandardCostHeaders_StdCostId",
                        column: x => x.StdCostId,
                        principalSchema: "cost",
                        principalTable: "StandardCostHeaders",
                        principalColumn: "StdCostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StandardCostHeaders_ProductCode_CostVersion",
                schema: "cost",
                table: "StandardCostHeaders",
                columns: new[] { "ProductCode", "CostVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardCostHeaders_Status",
                schema: "cost",
                table: "StandardCostHeaders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StandardCostMaterialLines_StdCostId",
                schema: "cost",
                table: "StandardCostMaterialLines",
                column: "StdCostId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardCostRoutingLines_StdCostId",
                schema: "cost",
                table: "StandardCostRoutingLines",
                column: "StdCostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StandardCostMaterialLines",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "StandardCostRoutingLines",
                schema: "cost");

            migrationBuilder.DropTable(
                name: "StandardCostHeaders",
                schema: "cost");
        }
    }
}
