using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityStandardSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityStandardSets",
                schema: "qual",
                columns: table => new
                {
                    StandardSetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SamplingMethodID = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProductionProcessId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityStandardSets", x => x.StandardSetID);
                });

            migrationBuilder.CreateTable(
                name: "QualityStandardSetCriteria",
                schema: "qual",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StandardSetID = table.Column<int>(type: "int", nullable: false),
                    CriteriaID = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityStandardSetCriteria", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QualityStandardSetCriteria_QualityStandardSets_StandardSetID",
                        column: x => x.StandardSetID,
                        principalSchema: "qual",
                        principalTable: "QualityStandardSets",
                        principalColumn: "StandardSetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityStandardSetStageCriteria",
                schema: "qual",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StandardSetID = table.Column<int>(type: "int", nullable: false),
                    ProductionStageID = table.Column<int>(type: "int", nullable: false),
                    CriteriaID = table.Column<int>(type: "int", nullable: false),
                    SamplingMethodID = table.Column<int>(type: "int", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityStandardSetStageCriteria", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QualityStandardSetStageCriteria_QualityStandardSets_StandardSetID",
                        column: x => x.StandardSetID,
                        principalSchema: "qual",
                        principalTable: "QualityStandardSets",
                        principalColumn: "StandardSetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityStandardSetCriteria_StandardSetID",
                schema: "qual",
                table: "QualityStandardSetCriteria",
                column: "StandardSetID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityStandardSets_Code",
                schema: "qual",
                table: "QualityStandardSets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityStandardSetStageCriteria_StandardSetID",
                schema: "qual",
                table: "QualityStandardSetStageCriteria",
                column: "StandardSetID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityStandardSetCriteria",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "QualityStandardSetStageCriteria",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "QualityStandardSets",
                schema: "qual");
        }
    }
}
