using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectCodeSeverityAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "qual",
                table: "DefectCodes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeverityLevel",
                schema: "qual",
                table: "DefectCodes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Minor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.DropColumn(
                name: "SeverityLevel",
                schema: "qual",
                table: "DefectCodes");
        }
    }
}
