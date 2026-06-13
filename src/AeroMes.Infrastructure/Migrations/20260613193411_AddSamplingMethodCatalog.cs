using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSamplingMethodCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SamplingMethods",
                schema: "qual",
                columns: table => new
                {
                    SamplingMethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SamplingType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SampleQuantity = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: true),
                    MaxDefects = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SamplingMethods", x => x.SamplingMethodID);
                });

            migrationBuilder.CreateTable(
                name: "SamplingVolumeRanges",
                schema: "qual",
                columns: table => new
                {
                    RangeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SamplingMethodID = table.Column<int>(type: "int", nullable: false),
                    MinQty = table.Column<int>(type: "int", nullable: false),
                    MaxQty = table.Column<int>(type: "int", nullable: false),
                    SampleSizeOrRatio = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    MaxDefects = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamplingVolumeRanges", x => x.RangeID);
                    table.ForeignKey(
                        name: "FK_SamplingVolumeRanges_SamplingMethods_SamplingMethodID",
                        column: x => x.SamplingMethodID,
                        principalSchema: "qual",
                        principalTable: "SamplingMethods",
                        principalColumn: "SamplingMethodID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SamplingMethods_Code",
                schema: "qual",
                table: "SamplingMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SamplingVolumeRanges_SamplingMethodID",
                schema: "qual",
                table: "SamplingVolumeRanges",
                column: "SamplingMethodID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SamplingVolumeRanges",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "SamplingMethods",
                schema: "qual");
        }
    }
}
