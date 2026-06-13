using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecallAuditLog",
                schema: "trace",
                columns: table => new
                {
                    AuditID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecallID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActionDetail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    SystemGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallAuditLog", x => x.AuditID);
                });

            migrationBuilder.CreateTable(
                name: "Recalls",
                schema: "trace",
                columns: table => new
                {
                    RecallID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecallCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecallType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnchorLotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnchorDirection = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RegulatoryRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InitiatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ScopeIdentifiedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recalls", x => x.RecallID);
                });

            migrationBuilder.CreateTable(
                name: "RecallScopeLots",
                schema: "trace",
                columns: table => new
                {
                    RecallScopeLotID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecallID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TraceDepth = table.Column<int>(type: "int", nullable: false),
                    LotCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentLocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QtyOnHand = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: true),
                    ShipmentRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HoldID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallScopeLots", x => x.RecallScopeLotID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecallAuditLog_RecallID",
                schema: "trace",
                table: "RecallAuditLog",
                column: "RecallID");

            migrationBuilder.CreateIndex(
                name: "IX_Recalls_RecallCode",
                schema: "trace",
                table: "Recalls",
                column: "RecallCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recalls_Status_InitiatedAt",
                schema: "trace",
                table: "Recalls",
                columns: new[] { "Status", "InitiatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecallScopeLots_RecallID_LotNumber",
                schema: "trace",
                table: "RecallScopeLots",
                columns: new[] { "RecallID", "LotNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecallAuditLog",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "Recalls",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "RecallScopeLots",
                schema: "trace");
        }
    }
}
