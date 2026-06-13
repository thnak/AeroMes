using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdapterHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdapterHealthLogs",
                schema: "iot",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdapterId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdapterHealthLogs", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "AdapterHealths",
                schema: "iot",
                columns: table => new
                {
                    AdapterId = table.Column<int>(type: "int", nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdapterType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSignalAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignalRate1min = table.Column<double>(type: "float", nullable: false),
                    ErrorCount1hr = table.Column<int>(type: "int", nullable: false),
                    ReconnectAttempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdapterHealths", x => x.AdapterId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdapterHealthLogs_AdapterId_EventAt",
                schema: "iot",
                table: "AdapterHealthLogs",
                columns: new[] { "AdapterId", "EventAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AdapterHealths_MachineCode",
                schema: "iot",
                table: "AdapterHealths",
                column: "MachineCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdapterHealthLogs",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "AdapterHealths",
                schema: "iot");
        }
    }
}
