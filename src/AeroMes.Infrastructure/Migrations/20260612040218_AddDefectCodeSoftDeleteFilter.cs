using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectCodeSoftDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefectCodes_Code",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.CreateIndex(
                name: "IX_DefectCodes_Code",
                schema: "qual",
                table: "DefectCodes",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefectCodes_Code",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.CreateIndex(
                name: "IX_DefectCodes_Code",
                schema: "qual",
                table: "DefectCodes",
                column: "Code",
                unique: true);
        }
    }
}
