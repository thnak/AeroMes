using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkShiftAndWorkCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkCalendars",
                schema: "master",
                columns: table => new
                {
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalendarName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WorkCalendars", x => x.WorkCalendarId);
                });

            migrationBuilder.CreateTable(
                name: "WorkShifts",
                schema: "master",
                columns: table => new
                {
                    WorkShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    NetMinutes = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WorkShifts", x => x.WorkShiftId);
                });

            migrationBuilder.CreateTable(
                name: "CalendarDays",
                schema: "master",
                columns: table => new
                {
                    CalendarDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDays", x => x.CalendarDayId);
                    table.ForeignKey(
                        name: "FK_CalendarDays_WorkCalendars_WorkCalendarId",
                        column: x => x.WorkCalendarId,
                        principalSchema: "master",
                        principalTable: "WorkCalendars",
                        principalColumn: "WorkCalendarId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarExceptions",
                schema: "master",
                columns: table => new
                {
                    CalendarExceptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkShiftId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarExceptions", x => x.CalendarExceptionId);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_WorkCalendars_WorkCalendarId",
                        column: x => x.WorkCalendarId,
                        principalSchema: "master",
                        principalTable: "WorkCalendars",
                        principalColumn: "WorkCalendarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftBreaks",
                schema: "master",
                columns: table => new
                {
                    ShiftBreakId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkShiftId = table.Column<int>(type: "int", nullable: false),
                    BreakStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakEnd = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftBreaks", x => x.ShiftBreakId);
                    table.ForeignKey(
                        name: "FK_ShiftBreaks_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarShifts",
                schema: "master",
                columns: table => new
                {
                    CalendarShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarDayId = table.Column<int>(type: "int", nullable: false),
                    WorkShiftId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarShifts", x => x.CalendarShiftId);
                    table.ForeignKey(
                        name: "FK_CalendarShifts_CalendarDays_CalendarDayId",
                        column: x => x.CalendarDayId,
                        principalSchema: "master",
                        principalTable: "CalendarDays",
                        principalColumn: "CalendarDayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarShifts_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDays_WorkCalendarId_DayOfWeek",
                schema: "master",
                table: "CalendarDays",
                columns: new[] { "WorkCalendarId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_WorkCalendarId_Date",
                schema: "master",
                table: "CalendarExceptions",
                columns: new[] { "WorkCalendarId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_WorkShiftId",
                schema: "master",
                table: "CalendarExceptions",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShifts_CalendarDayId",
                schema: "master",
                table: "CalendarShifts",
                column: "CalendarDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShifts_WorkShiftId",
                schema: "master",
                table: "CalendarShifts",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftBreaks_WorkShiftId",
                schema: "master",
                table: "ShiftBreaks",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCalendars_CalendarCode",
                schema: "master",
                table: "WorkCalendars",
                column: "CalendarCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_ShiftCode",
                schema: "master",
                table: "WorkShifts",
                column: "ShiftCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarExceptions",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CalendarShifts",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ShiftBreaks",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CalendarDays",
                schema: "master");

            migrationBuilder.DropTable(
                name: "WorkShifts",
                schema: "master");

            migrationBuilder.DropTable(
                name: "WorkCalendars",
                schema: "master");
        }
    }
}
