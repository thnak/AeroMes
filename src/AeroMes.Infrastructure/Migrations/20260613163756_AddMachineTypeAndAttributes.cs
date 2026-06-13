using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineTypeAndAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomAttributes",
                schema: "master",
                table: "Machines",
                type: "NVARCHAR(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineType",
                schema: "master",
                table: "Machines",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "GENERAL");

            migrationBuilder.AddColumn<int>(
                name: "ClampingForceTons",
                schema: "master",
                table: "Machines",
                type: "int",
                nullable: true,
                computedColumnSql: "CAST(JSON_VALUE(CustomAttributes, '$.clamping_force_tons') AS INT)",
                stored: false);

            migrationBuilder.AddColumn<string>(
                name: "SewingMachineClass",
                schema: "master",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "JSON_VALUE(CustomAttributes, '$.machine_class')",
                stored: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClampingForceTons",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SewingMachineClass",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CustomAttributes",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineType",
                schema: "master",
                table: "Machines");
        }
    }
}
