using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialRequirementsPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialRequirementsPlans",
                schema: "prod",
                columns: table => new
                {
                    MrpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MasterPlanId = table.Column<int>(type: "int", nullable: true),
                    OrganizationalUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MaterialRequirementsPlans", x => x.MrpID);
                });

            migrationBuilder.CreateTable(
                name: "MrpLines",
                schema: "prod",
                columns: table => new
                {
                    MrpLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MrpID = table.Column<int>(type: "int", nullable: false),
                    FinishedGoodCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinishedGoodQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FixedWaste = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    WasteRatio = table.Column<decimal>(type: "DECIMAL(10,6)", nullable: false),
                    CalculatedMaterialQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    OpeningInventory = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ConcurrentPurchaseRequestQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    PlannedOrderQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrpLines", x => x.MrpLineID);
                    table.ForeignKey(
                        name: "FK_MrpLines_MaterialRequirementsPlans_MrpID",
                        column: x => x.MrpID,
                        principalSchema: "prod",
                        principalTable: "MaterialRequirementsPlans",
                        principalColumn: "MrpID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequirementsPlans_PlanNumber",
                schema: "prod",
                table: "MaterialRequirementsPlans",
                column: "PlanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MrpLines_MrpID",
                schema: "prod",
                table: "MrpLines",
                column: "MrpID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MrpLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MaterialRequirementsPlans",
                schema: "prod");
        }
    }
}
