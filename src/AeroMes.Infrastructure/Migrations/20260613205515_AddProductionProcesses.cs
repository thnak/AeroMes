using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionProcesses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionProcesses",
                schema: "master",
                columns: table => new
                {
                    ProcessID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplicationScope = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductGroupIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsForPlanningOnly = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ProductionProcesses", x => x.ProcessID);
                });

            migrationBuilder.CreateTable(
                name: "ProductionProcessStages",
                schema: "master",
                columns: table => new
                {
                    StageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessID = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ProcessStepCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CapacityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CapacityIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedTimeSeconds = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    PlannedTimeSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeOffsetDays = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryStage = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionProcessStages", x => x.StageID);
                    table.ForeignKey(
                        name: "FK_ProductionProcessStages_ProductionProcesses_ProcessID",
                        column: x => x.ProcessID,
                        principalSchema: "master",
                        principalTable: "ProductionProcesses",
                        principalColumn: "ProcessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionProcesses_Code",
                schema: "master",
                table: "ProductionProcesses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionProcessStages_ProcessID",
                schema: "master",
                table: "ProductionProcessStages",
                column: "ProcessID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionProcessStages",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ProductionProcesses",
                schema: "master");
        }
    }
}
