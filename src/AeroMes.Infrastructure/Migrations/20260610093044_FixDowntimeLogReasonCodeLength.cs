using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDowntimeLogReasonCodeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_DowntimeLogs_DowntimeReasonCodes_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                column: "ReasonCode",
                principalSchema: "master",
                principalTable: "DowntimeReasonCodes",
                principalColumn: "ReasonCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DowntimeLogs_DowntimeReasonCodes_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
