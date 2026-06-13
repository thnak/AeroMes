using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerialAggregations",
                schema: "trace",
                columns: table => new
                {
                    AggregationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildSerialID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChildSSCC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ParentSSCC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AggregationType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AggregatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DisaggregatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialAggregations", x => x.AggregationID);
                });

            migrationBuilder.CreateTable(
                name: "SerialEvents",
                schema: "trace",
                columns: table => new
                {
                    EventID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SerialID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialEvents", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "SerialLotLineage",
                schema: "trace",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentLotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: true),
                    UoM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoutingStepID = table.Column<int>(type: "int", nullable: true),
                    AssembledAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialLotLineage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SerialUnits",
                schema: "trace",
                columns: table => new
                {
                    SerialID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    ProductionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UDI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialUnits", x => x.SerialID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SA_Child_Active",
                schema: "trace",
                table: "SerialAggregations",
                column: "ChildSerialID",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SA_Parent_Active",
                schema: "trace",
                table: "SerialAggregations",
                column: "ParentSSCC",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SerialEvents_SerialID_EventTimestamp",
                schema: "trace",
                table: "SerialEvents",
                columns: new[] { "SerialID", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SerialEvents_WorkOrderID",
                schema: "trace",
                table: "SerialEvents",
                column: "WorkOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_SerialLotLineage_ComponentLotNumber",
                schema: "trace",
                table: "SerialLotLineage",
                column: "ComponentLotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SerialLotLineage_SerialID",
                schema: "trace",
                table: "SerialLotLineage",
                column: "SerialID");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_LotNumber",
                schema: "trace",
                table: "SerialUnits",
                column: "LotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_ProductCode_Status",
                schema: "trace",
                table: "SerialUnits",
                columns: new[] { "ProductCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_SerialNumber",
                schema: "trace",
                table: "SerialUnits",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_WorkOrderID",
                schema: "trace",
                table: "SerialUnits",
                column: "WorkOrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialAggregations",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "SerialEvents",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "SerialLotLineage",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "SerialUnits",
                schema: "trace");
        }
    }
}
