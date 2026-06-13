using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lab");

            migrationBuilder.CreateTable(
                name: "LabReports",
                schema: "lab",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    OverallResult = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Conclusion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IssuedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabReports", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "LabRequests",
                schema: "lab",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkOrderId = table.Column<long>(type: "bigint", nullable: true),
                    InspectionOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PanelId = table.Column<int>(type: "int", nullable: true),
                    SampleQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SampleUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SampleLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequiredBy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabRequests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "LabSamples",
                schema: "lab",
                columns: table => new
                {
                    SampleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    SampleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConditionOnReceipt = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisposedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DisposalMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabSamples", x => x.SampleId);
                });

            migrationBuilder.CreateTable(
                name: "TestMethods",
                schema: "lab",
                columns: table => new
                {
                    TestMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MeasurementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SpecMin = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SpecMax = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SpecNominal = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReferenceStd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InstrumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstDurationMin = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestMethods", x => x.TestMethodId);
                });

            migrationBuilder.CreateTable(
                name: "TestPanels",
                schema: "lab",
                columns: table => new
                {
                    PanelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPanels", x => x.PanelId);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                schema: "lab",
                columns: table => new
                {
                    ResultId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    SampleId = table.Column<long>(type: "bigint", nullable: false),
                    TestMethodId = table.Column<int>(type: "int", nullable: false),
                    MeasuredValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AttributeResult = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsWithinSpec = table.Column<bool>(type: "bit", nullable: true),
                    InstrumentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TestedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.ResultId);
                });

            migrationBuilder.CreateTable(
                name: "TestPanelItems",
                schema: "lab",
                columns: table => new
                {
                    PanelItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PanelId = table.Column<int>(type: "int", nullable: false),
                    TestMethodId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SpecOverrideMin = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SpecOverrideMax = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPanelItems", x => x.PanelItemId);
                    table.ForeignKey(
                        name: "FK_TestPanelItems_TestPanels_PanelId",
                        column: x => x.PanelId,
                        principalSchema: "lab",
                        principalTable: "TestPanels",
                        principalColumn: "PanelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabReports_ReportNo",
                schema: "lab",
                table: "LabReports",
                column: "ReportNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabRequests_RequestNo",
                schema: "lab",
                table: "LabRequests",
                column: "RequestNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabRequests_Status_RequestedAt",
                schema: "lab",
                table: "LabRequests",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LabSamples_SampleCode",
                schema: "lab",
                table: "LabSamples",
                column: "SampleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestMethods_Code",
                schema: "lab",
                table: "TestMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestPanelItems_PanelId",
                schema: "lab",
                table: "TestPanelItems",
                column: "PanelId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPanels_Code",
                schema: "lab",
                table: "TestPanels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_RequestId_SampleId",
                schema: "lab",
                table: "TestResults",
                columns: new[] { "RequestId", "SampleId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabReports",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "LabRequests",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "LabSamples",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "TestMethods",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "TestPanelItems",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "TestResults",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "TestPanels",
                schema: "lab");
        }
    }
}
