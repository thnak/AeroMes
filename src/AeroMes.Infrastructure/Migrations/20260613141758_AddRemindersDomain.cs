using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemindersDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reminders");

            migrationBuilder.CreateTable(
                name: "ReminderAlerts",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderConfigurations",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReminderType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    NotificationChannel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReminderAlerts_ReminderType_EntityId_IsRead",
                schema: "reminders",
                table: "ReminderAlerts",
                columns: new[] { "ReminderType", "EntityId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ReminderConfigurations_UserId_ReminderType",
                schema: "reminders",
                table: "ReminderConfigurations",
                columns: new[] { "UserId", "ReminderType" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderAlerts",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "ReminderConfigurations",
                schema: "reminders");
        }
    }
}
