using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineStateEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineStateHistories",
                schema: "iot",
                columns: table => new
                {
                    HistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransitionAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    TriggerRuleId = table.Column<int>(type: "int", nullable: true),
                    TriggerTagKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TriggerValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStateHistories", x => x.HistoryId);
                });

            migrationBuilder.CreateTable(
                name: "MachineStateSnapshots",
                schema: "iot",
                columns: table => new
                {
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreviousState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TriggerRuleId = table.Column<int>(type: "int", nullable: true),
                    TriggerTagKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TriggerValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SignalStaleSince = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSignalAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStateSnapshots", x => x.MachineCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateHistories_MachineCode_TransitionAt",
                schema: "iot",
                table: "MachineStateHistories",
                columns: new[] { "MachineCode", "TransitionAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineStateHistories",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "MachineStateSnapshots",
                schema: "iot");
        }
    }
}
