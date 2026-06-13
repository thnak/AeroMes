using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSopDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sop");

            migrationBuilder.CreateTable(
                name: "ChecksheetInstances",
                schema: "sop",
                columns: table => new
                {
                    InstanceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SopId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    WorkOrderId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OperatorCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecksheetInstances", x => x.InstanceId);
                });

            migrationBuilder.CreateTable(
                name: "SopDocuments",
                schema: "sop",
                columns: table => new
                {
                    SopId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RoutingStepId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SopDocuments", x => x.SopId);
                });

            migrationBuilder.CreateTable(
                name: "CheckItemResults",
                schema: "sop",
                columns: table => new
                {
                    ResultId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<long>(type: "bigint", nullable: false),
                    CheckItemId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletionSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MeasuredValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckItemResults", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_CheckItemResults_ChecksheetInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalSchema: "sop",
                        principalTable: "ChecksheetInstances",
                        principalColumn: "InstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckItems",
                schema: "sop",
                columns: table => new
                {
                    CheckItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SopId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CompletionMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AutoConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecMin = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SpecMax = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhotoRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckItems", x => x.CheckItemId);
                    table.ForeignKey(
                        name: "FK_CheckItems_SopDocuments_SopId",
                        column: x => x.SopId,
                        principalSchema: "sop",
                        principalTable: "SopDocuments",
                        principalColumn: "SopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckItemResults_InstanceId_CheckItemId",
                schema: "sop",
                table: "CheckItemResults",
                columns: new[] { "InstanceId", "CheckItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckItems_SopId",
                schema: "sop",
                table: "CheckItems",
                column: "SopId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecksheetInstances_JobId",
                schema: "sop",
                table: "ChecksheetInstances",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecksheetInstances_WorkOrderId",
                schema: "sop",
                table: "ChecksheetInstances",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SopDocuments_Code",
                schema: "sop",
                table: "SopDocuments",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SopDocuments_RoutingStepId_Status",
                schema: "sop",
                table: "SopDocuments",
                columns: new[] { "RoutingStepId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckItemResults",
                schema: "sop");

            migrationBuilder.DropTable(
                name: "CheckItems",
                schema: "sop");

            migrationBuilder.DropTable(
                name: "ChecksheetInstances",
                schema: "sop");

            migrationBuilder.DropTable(
                name: "SopDocuments",
                schema: "sop");
        }
    }
}
