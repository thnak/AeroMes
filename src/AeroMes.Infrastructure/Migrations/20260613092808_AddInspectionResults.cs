using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionResults",
                schema: "qual",
                columns: table => new
                {
                    ResultId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionOrderId = table.Column<int>(type: "int", nullable: false),
                    CharId = table.Column<int>(type: "int", nullable: false),
                    MeasuredValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AttributeResult = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsWithinSpec = table.Column<bool>(type: "bit", nullable: true),
                    DefectCodeId = table.Column<int>(type: "int", nullable: true),
                    SampleIndex = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RecordedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResults", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_InspectionResults_InspectionCharacteristics_CharId",
                        column: x => x.CharId,
                        principalSchema: "qual",
                        principalTable: "InspectionCharacteristics",
                        principalColumn: "CharId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_CharId",
                schema: "qual",
                table: "InspectionResults",
                column: "CharId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_InspectionOrderId_CharId",
                schema: "qual",
                table: "InspectionResults",
                columns: new[] { "InspectionOrderId", "CharId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionResults",
                schema: "qual");
        }
    }
}
