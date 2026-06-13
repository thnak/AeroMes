using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionSchedules",
                schema: "prod",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacilityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_ProductionSchedules", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionScheduleLines",
                schema: "prod",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    POID = table.Column<int>(type: "int", nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionScheduleLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleLines_ProductionSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "prod",
                        principalTable: "ProductionSchedules",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleLines_ScheduleId",
                schema: "prod",
                table: "ProductionScheduleLines",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleLines_ScheduleId_POID",
                schema: "prod",
                table: "ProductionScheduleLines",
                columns: new[] { "ScheduleId", "POID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionScheduleLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "ProductionSchedules",
                schema: "prod");
        }
    }
}
