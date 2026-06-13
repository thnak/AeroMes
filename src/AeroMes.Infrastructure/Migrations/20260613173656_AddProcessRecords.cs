using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessRecords",
                schema: "trace",
                columns: table => new
                {
                    ProcessRecordID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkOrderID = table.Column<int>(type: "int", nullable: false),
                    JobID = table.Column<long>(type: "bigint", nullable: true),
                    RoutingStepID = table.Column<int>(type: "int", nullable: false),
                    StepSequence = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperatorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CertificationRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CalibrationRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BOMRevision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoutingRevision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ControlPlanRev = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StepStartedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    StepCompletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    StepOutcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeviationRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRecords", x => x.ProcessRecordID);
                });

            migrationBuilder.CreateTable(
                name: "ProcessParameters",
                schema: "trace",
                columns: table => new
                {
                    ParameterID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessRecordID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParameterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NominalValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActualValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UoM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LSL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    USL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InSpec = table.Column<bool>(type: "bit", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessParameters", x => x.ParameterID);
                    table.ForeignKey(
                        name: "FK_ProcessParameters_ProcessRecords_ProcessRecordID",
                        column: x => x.ProcessRecordID,
                        principalSchema: "trace",
                        principalTable: "ProcessRecords",
                        principalColumn: "ProcessRecordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessParameters_ProcessRecordID",
                schema: "trace",
                table: "ProcessParameters",
                column: "ProcessRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecords_LotNumber_RoutingStepID",
                schema: "trace",
                table: "ProcessRecords",
                columns: new[] { "LotNumber", "RoutingStepID" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecords_WorkOrderID_StepSequence",
                schema: "trace",
                table: "ProcessRecords",
                columns: new[] { "WorkOrderID", "StepSequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessParameters",
                schema: "trace");

            migrationBuilder.DropTable(
                name: "ProcessRecords",
                schema: "trace");
        }
    }
}
