using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sched");

            migrationBuilder.CreateTable(
                name: "CapacityCalendars",
                schema: "sched",
                columns: table => new
                {
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    CalendarDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftTemplateId = table.Column<int>(type: "int", nullable: false),
                    AvailableMinutes = table.Column<int>(type: "int", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacityCalendars", x => new { x.WorkCenterID, x.CalendarDate, x.ShiftTemplateId });
                    table.ForeignKey(
                        name: "FK_CapacityCalendars_WorkCenters_WorkCenterID",
                        column: x => x.WorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapacityCalendars_CalendarDate_WorkCenterID",
                schema: "sched",
                table: "CapacityCalendars",
                columns: new[] { "CalendarDate", "WorkCenterID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapacityCalendars",
                schema: "sched");
        }
    }
}
