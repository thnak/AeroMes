using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionPlansCharacteristics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionPlans",
                schema: "qual",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoutingStepId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SamplingMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: true),
                    AcceptNumber = table.Column<int>(type: "int", nullable: false),
                    RejectNumber = table.Column<int>(type: "int", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "InspectionCharacteristics",
                schema: "qual",
                columns: table => new
                {
                    CharId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CharName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MeasurementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SpecMin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpecMax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpecNominal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AttributeSpec = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefectCodeLink = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionCharacteristics", x => x.CharId);
                    table.ForeignKey(
                        name: "FK_InspectionCharacteristics_InspectionPlans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "qual",
                        principalTable: "InspectionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionCharacteristics_PlanId",
                schema: "qual",
                table: "InspectionCharacteristics",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlans_Code",
                schema: "qual",
                table: "InspectionPlans",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionCharacteristics",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "InspectionPlans",
                schema: "qual");
        }
    }
}
