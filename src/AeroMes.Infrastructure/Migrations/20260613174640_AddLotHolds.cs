using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLotHolds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LotHolds",
                schema: "trace",
                columns: table => new
                {
                    HoldID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    HoldStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoldReason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HoldDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HoldReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HoldInitiatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoldInitiatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DispositionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DispositionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReleasedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ESignatureRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotHolds", x => x.HoldID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LH_Lot_Active",
                schema: "trace",
                table: "LotHolds",
                column: "LotNumber",
                filter: "[HoldStatus] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_LH_Status_Reason",
                schema: "trace",
                table: "LotHolds",
                columns: new[] { "HoldStatus", "HoldReason", "HoldInitiatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LH_WO_Active",
                schema: "trace",
                table: "LotHolds",
                column: "WorkOrderID",
                filter: "[HoldStatus] = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotHolds",
                schema: "trace");
        }
    }
}
