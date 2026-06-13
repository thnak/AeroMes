using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLotTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trace");

            migrationBuilder.CreateTable(
                name: "LotEvents",
                schema: "trace",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    RoutingStepID = table.Column<int>(type: "int", nullable: true),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UoM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EquipmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    SourceSystem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "LotLineages",
                schema: "trace",
                columns: table => new
                {
                    LineageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentLotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChildLotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    RoutingStepID = table.Column<int>(type: "int", nullable: true),
                    LineageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuantityConsumed = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: true),
                    UoM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotLineages", x => x.LineageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotEvents_LotNumber_EventTimestamp",
                schema: "trace",
                table: "LotEvents",
                columns: new[] { "LotNumber", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_LotEvents_ProductCode",
                schema: "trace",
                table: "LotEvents",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_LotLineages_ChildLotNumber",
                schema: "trace",
                table: "LotLineages",
                column: "ChildLotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LotLineages_ParentLotNumber",
                schema: "trace",
                table: "LotLineages",
                column: "ParentLotNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotEvents",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "LotLineages",
                schema: "trace");
        }
    }
}
