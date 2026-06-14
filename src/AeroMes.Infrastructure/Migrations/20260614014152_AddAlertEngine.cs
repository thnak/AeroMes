using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "alert");

            migrationBuilder.AddColumn<int>(
                name: "CooldownMinutes",
                schema: "master",
                table: "AlertThresholds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                schema: "master",
                table: "AlertThresholds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AlertEvents",
                schema: "alert",
                columns: table => new
                {
                    AlertEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThresholdId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MetricValue = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEvents", x => x.AlertEventId);
                    table.ForeignKey(
                        name: "FK_AlertEvents_AlertThresholds_ThresholdId",
                        column: x => x.ThresholdId,
                        principalSchema: "master",
                        principalTable: "AlertThresholds",
                        principalColumn: "ThresholdId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_IsActive",
                schema: "alert",
                table: "AlertEvents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_ThresholdId_TriggeredAt",
                schema: "alert",
                table: "AlertEvents",
                columns: new[] { "ThresholdId", "TriggeredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertEvents",
                schema: "alert");

            migrationBuilder.DropColumn(
                name: "CooldownMinutes",
                schema: "master",
                table: "AlertThresholds");

            migrationBuilder.DropColumn(
                name: "EmailEnabled",
                schema: "master",
                table: "AlertThresholds");
        }
    }
}
