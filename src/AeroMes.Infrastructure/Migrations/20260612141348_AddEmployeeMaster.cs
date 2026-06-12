using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireCertification",
                schema: "master",
                table: "WorkOrderAutoRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "master",
                columns: table => new
                {
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultWorkCenterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Employees", x => x.EmployeeCode);
                    table.ForeignKey(
                        name: "FK_Employees_WorkCenters_DefaultWorkCenterId",
                        column: x => x.DefaultWorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSkills",
                schema: "master",
                columns: table => new
                {
                    EmployeeSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CertificationLevel = table.Column<int>(type: "int", nullable: false),
                    CertifiedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresAt = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSkills", x => x.EmployeeSkillId);
                    table.ForeignKey(
                        name: "FK_EmployeeSkills_Employees_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalSchema: "master",
                        principalTable: "Employees",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSkills_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalSchema: "master",
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAssignments",
                schema: "master",
                columns: table => new
                {
                    ShiftAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAssignments", x => x.ShiftAssignmentId);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_Employees_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalSchema: "master",
                        principalTable: "Employees",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_ShiftTemplates_ShiftCode",
                        column: x => x.ShiftCode,
                        principalSchema: "master",
                        principalTable: "ShiftTemplates",
                        principalColumn: "ShiftCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OperatorID",
                schema: "prod",
                table: "Jobs",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DefaultWorkCenterId",
                schema: "master",
                table: "Employees",
                column: "DefaultWorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSkills_EmployeeCode_OperationCode",
                schema: "master",
                table: "EmployeeSkills",
                columns: new[] { "EmployeeCode", "OperationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSkills_OperationCode",
                schema: "master",
                table: "EmployeeSkills",
                column: "OperationCode");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_EmployeeCode_ValidFrom",
                schema: "master",
                table: "ShiftAssignments",
                columns: new[] { "EmployeeCode", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_ShiftCode",
                schema: "master",
                table: "ShiftAssignments",
                column: "ShiftCode");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_WorkCenterId",
                schema: "master",
                table: "ShiftAssignments",
                column: "WorkCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Employees_OperatorID",
                schema: "prod",
                table: "Jobs",
                column: "OperatorID",
                principalSchema: "master",
                principalTable: "Employees",
                principalColumn: "EmployeeCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Employees_OperatorID",
                schema: "prod",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "EmployeeSkills",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ShiftAssignments",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_OperatorID",
                schema: "prod",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequireCertification",
                schema: "master",
                table: "WorkOrderAutoRules");
        }
    }
}
