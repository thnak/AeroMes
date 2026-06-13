using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddErpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErpApiKey",
                schema: "settings",
                table: "SystemOptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpBaseUrl",
                schema: "settings",
                table: "SystemOptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ErpEnabled",
                schema: "settings",
                table: "SystemOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ErpLastSyncAt",
                schema: "settings",
                table: "SystemOptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ErpSyncIntervalMinutes",
                schema: "settings",
                table: "SystemOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErpApiKey",
                schema: "settings",
                table: "SystemOptions");

            migrationBuilder.DropColumn(
                name: "ErpBaseUrl",
                schema: "settings",
                table: "SystemOptions");

            migrationBuilder.DropColumn(
                name: "ErpEnabled",
                schema: "settings",
                table: "SystemOptions");

            migrationBuilder.DropColumn(
                name: "ErpLastSyncAt",
                schema: "settings",
                table: "SystemOptions");

            migrationBuilder.DropColumn(
                name: "ErpSyncIntervalMinutes",
                schema: "settings",
                table: "SystemOptions");
        }
    }
}
