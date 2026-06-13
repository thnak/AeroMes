using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialBlendLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialBlendLog",
                schema: "prod",
                columns: table => new
                {
                    BlendLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    ResinProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VirginLotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VirginQtyKg = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    RegrindLotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegrindQtyKg = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    MaxAllowedPct = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialBlendLog", x => x.BlendLogID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBlendLog_JobID_RecordedAt",
                schema: "prod",
                table: "MaterialBlendLog",
                columns: new[] { "JobID", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBlendLog_ResinProductCode",
                schema: "prod",
                table: "MaterialBlendLog",
                column: "ResinProductCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialBlendLog",
                schema: "prod");
        }
    }
}
