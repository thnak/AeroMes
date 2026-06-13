using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDHUQualityInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMajorDefault",
                schema: "qual",
                table: "DefectCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AQLInspections",
                schema: "qual",
                columns: table => new
                {
                    AQLInspectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    AQLLevel = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    InspectionLevel = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    LotSize = table.Column<int>(type: "int", nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    AcceptanceNumber = table.Column<int>(type: "int", nullable: false),
                    RejectionNumber = table.Column<int>(type: "int", nullable: false),
                    DefectsFound = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "PENDING"),
                    InspectorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AQLInspections", x => x.AQLInspectionID);
                });

            migrationBuilder.CreateTable(
                name: "InlineInspections",
                schema: "qual",
                columns: table => new
                {
                    InspectionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InspectorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    TotalDefects = table.Column<int>(type: "int", nullable: false),
                    DHU = table.Column<decimal>(type: "DECIMAL(8,4)", nullable: false, computedColumnSql: "CAST([TotalDefects] AS float) / [SampleSize] * 100.0", stored: true),
                    DHU_Target = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false, defaultValue: 2.5m),
                    IsAboveTarget = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN CAST([TotalDefects] AS float) / [SampleSize] * 100.0 > [DHU_Target] THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InlineInspections", x => x.InspectionID);
                });

            migrationBuilder.CreateTable(
                name: "AQLInspectionDefects",
                schema: "qual",
                columns: table => new
                {
                    DefectID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AQLInspectionID = table.Column<int>(type: "int", nullable: false),
                    DefectCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsMajor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AQLInspectionDefects", x => x.DefectID);
                    table.ForeignKey(
                        name: "FK_AQLInspectionDefects_AQLInspections_AQLInspectionID",
                        column: x => x.AQLInspectionID,
                        principalSchema: "qual",
                        principalTable: "AQLInspections",
                        principalColumn: "AQLInspectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InlineInspectionDefects",
                schema: "qual",
                columns: table => new
                {
                    DefectID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionID = table.Column<long>(type: "bigint", nullable: false),
                    DefectCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    DefectLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsMajor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InlineInspectionDefects", x => x.DefectID);
                    table.ForeignKey(
                        name: "FK_InlineInspectionDefects_InlineInspections_InspectionID",
                        column: x => x.InspectionID,
                        principalSchema: "qual",
                        principalTable: "InlineInspections",
                        principalColumn: "InspectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AQLInspectionDefects_AQLInspectionID",
                schema: "qual",
                table: "AQLInspectionDefects",
                column: "AQLInspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_AQLInspections_WOID",
                schema: "qual",
                table: "AQLInspections",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_InlineInspectionDefects_InspectionID",
                schema: "qual",
                table: "InlineInspectionDefects",
                column: "InspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_InlineInspections_IsAboveTarget",
                schema: "qual",
                table: "InlineInspections",
                column: "IsAboveTarget",
                filter: "[IsAboveTarget] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InlineInspections_WOID_InspectedAt",
                schema: "qual",
                table: "InlineInspections",
                columns: new[] { "WOID", "InspectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InlineInspections_WorkCenterID_InspectedAt",
                schema: "qual",
                table: "InlineInspections",
                columns: new[] { "WorkCenterID", "InspectedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AQLInspectionDefects",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "InlineInspectionDefects",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "AQLInspections",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "InlineInspections",
                schema: "qual");

            migrationBuilder.DropColumn(
                name: "IsMajorDefault",
                schema: "qual",
                table: "DefectCodes");
        }
    }
}
