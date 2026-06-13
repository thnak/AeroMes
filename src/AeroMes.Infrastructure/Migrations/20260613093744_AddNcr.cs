using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNcr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ncrs",
                schema: "qual",
                columns: table => new
                {
                    NcrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NcrNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InspectionOrderId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QtyAffected = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DispositionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DispositionSetBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DispositionSetAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreventiveAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ncrs", x => x.NcrId);
                });

            migrationBuilder.CreateTable(
                name: "NcrDefectLines",
                schema: "qual",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NcrId = table.Column<int>(type: "int", nullable: false),
                    DefectCodeId = table.Column<int>(type: "int", nullable: false),
                    QtyDefective = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NcrDefectLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_NcrDefectLines_DefectCodes_DefectCodeId",
                        column: x => x.DefectCodeId,
                        principalSchema: "qual",
                        principalTable: "DefectCodes",
                        principalColumn: "DefectCodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NcrDefectLines_Ncrs_NcrId",
                        column: x => x.NcrId,
                        principalSchema: "qual",
                        principalTable: "Ncrs",
                        principalColumn: "NcrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NcrDefectLines_DefectCodeId",
                schema: "qual",
                table: "NcrDefectLines",
                column: "DefectCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NcrDefectLines_NcrId",
                schema: "qual",
                table: "NcrDefectLines",
                column: "NcrId");

            migrationBuilder.CreateIndex(
                name: "IX_Ncrs_NcrNo",
                schema: "qual",
                table: "Ncrs",
                column: "NcrNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NcrDefectLines",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "Ncrs",
                schema: "qual");
        }
    }
}
