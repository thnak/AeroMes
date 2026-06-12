using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionTeams",
                schema: "master",
                columns: table => new
                {
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrgUnitId = table.Column<int>(type: "int", nullable: true),
                    StandardLaborQuantity = table.Column<int>(type: "int", nullable: true),
                    ProductionRate = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    IsOrderBasedPlanningEnabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ProductionTeams", x => x.TeamCode);
                    table.ForeignKey(
                        name: "FK_ProductionTeams_OrgUnits_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalSchema: "master",
                        principalTable: "OrgUnits",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionTeamMembers",
                schema: "master",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLeader = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionTeamMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_ProductionTeamMembers_Employees_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalSchema: "master",
                        principalTable: "Employees",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionTeamMembers_ProductionTeams_TeamCode",
                        column: x => x.TeamCode,
                        principalSchema: "master",
                        principalTable: "ProductionTeams",
                        principalColumn: "TeamCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionTeamProductGroups",
                schema: "master",
                columns: table => new
                {
                    LinkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionTeamProductGroups", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_ProductionTeamProductGroups_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "master",
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionTeamProductGroups_ProductionTeams_TeamCode",
                        column: x => x.TeamCode,
                        principalSchema: "master",
                        principalTable: "ProductionTeams",
                        principalColumn: "TeamCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTeamMembers_EmployeeCode",
                schema: "master",
                table: "ProductionTeamMembers",
                column: "EmployeeCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTeamMembers_TeamCode_EmployeeCode",
                schema: "master",
                table: "ProductionTeamMembers",
                columns: new[] { "TeamCode", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTeamProductGroups_CategoryId",
                schema: "master",
                table: "ProductionTeamProductGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTeamProductGroups_TeamCode_CategoryId",
                schema: "master",
                table: "ProductionTeamProductGroups",
                columns: new[] { "TeamCode", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTeams_OrgUnitId",
                schema: "master",
                table: "ProductionTeams",
                column: "OrgUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionTeamMembers",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ProductionTeamProductGroups",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ProductionTeams",
                schema: "master");
        }
    }
}
