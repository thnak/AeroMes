using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoldManagementExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MoldCode",
                schema: "prod",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MoldAssignments",
                schema: "prod",
                columns: table => new
                {
                    AssignmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoldCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    MountedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnmountedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MountedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoldAssignments", x => x.AssignmentID);
                });

            migrationBuilder.CreateTable(
                name: "MoldMachineCompatibility",
                schema: "master",
                columns: table => new
                {
                    MoldCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompatible = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoldMachineCompatibility", x => new { x.MoldCode, x.MachineCode });
                });

            migrationBuilder.CreateTable(
                name: "MoldShotLog",
                schema: "prod",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoldCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    ShotsThisJob = table.Column<long>(type: "bigint", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoldShotLog", x => x.LogID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoldAssignments_MoldCode_MountedAt",
                schema: "prod",
                table: "MoldAssignments",
                columns: new[] { "MoldCode", "MountedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MoldAssignments_WOID",
                schema: "prod",
                table: "MoldAssignments",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_MoldShotLog_MoldCode_RecordedAt",
                schema: "prod",
                table: "MoldShotLog",
                columns: new[] { "MoldCode", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoldAssignments",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MoldMachineCompatibility",
                schema: "master");

            migrationBuilder.DropTable(
                name: "MoldShotLog",
                schema: "prod");

            migrationBuilder.DropColumn(
                name: "MoldCode",
                schema: "prod",
                table: "Jobs");
        }
    }
}
