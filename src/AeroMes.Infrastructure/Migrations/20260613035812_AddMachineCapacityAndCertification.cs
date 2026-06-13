using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineCapacityAndCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificationCode",
                schema: "master",
                table: "Machines",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyCostRate",
                schema: "master",
                table: "Machines",
                type: "DECIMAL(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineCategory",
                schema: "master",
                table: "Machines",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "MaxOperators",
                schema: "master",
                table: "Machines",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "OpcUaNodeId",
                schema: "master",
                table: "Machines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedDowntimeMinPerShift",
                schema: "master",
                table: "Machines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCertification",
                schema: "master",
                table: "Machines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetOeePct",
                schema: "master",
                table: "Machines",
                type: "NUMERIC(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TheoreticalCapacityPerHour",
                schema: "master",
                table: "Machines",
                type: "NUMERIC(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MachineProductParams",
                schema: "master",
                columns: table => new
                {
                    ParamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NominalValue = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    MinValue = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    IsControlParam = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineProductParams", x => x.ParamId);
                    table.ForeignKey(
                        name: "FK_MachineProductParams_Machines_MachineCode",
                        column: x => x.MachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineProductParams_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorCertifications",
                schema: "master",
                columns: table => new
                {
                    CertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CertificationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorCertifications", x => x.CertId);
                    table.ForeignKey(
                        name: "FK_OperatorCertifications_Employees_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalSchema: "master",
                        principalTable: "Employees",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductParams_MachineCode_ProductCode_ParamName",
                schema: "master",
                table: "MachineProductParams",
                columns: new[] { "MachineCode", "ProductCode", "ParamName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductParams_ProductCode",
                schema: "master",
                table: "MachineProductParams",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCertifications_EmployeeCode_CertificationCode_IsActive",
                schema: "master",
                table: "OperatorCertifications",
                columns: new[] { "EmployeeCode", "CertificationCode", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineProductParams",
                schema: "master");

            migrationBuilder.DropTable(
                name: "OperatorCertifications",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "CertificationCode",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "HourlyCostRate",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineCategory",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MaxOperators",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "OpcUaNodeId",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "PlannedDowntimeMinPerShift",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RequiresCertification",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "TargetOeePct",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "TheoreticalCapacityPerHour",
                schema: "master",
                table: "Machines");
        }
    }
}
