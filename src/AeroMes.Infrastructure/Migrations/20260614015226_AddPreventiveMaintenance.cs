using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventiveMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineRuntimeAccumulators",
                schema: "maint",
                columns: table => new
                {
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CumulativeRuntimeMinutes = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineRuntimeAccumulators", x => x.MachineCode);
                });

            migrationBuilder.CreateTable(
                name: "PmPlanTemplates",
                schema: "maint",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TriggerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggerInterval = table.Column<int>(type: "int", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
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
                    table.PrimaryKey("PK_PmPlanTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "PmChecklistItems",
                schema: "maint",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RequiresPartReplacement = table.Column<bool>(type: "bit", nullable: false),
                    PartCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmChecklistItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_PmChecklistItems_PmPlanTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "maint",
                        principalTable: "PmPlanTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PmWorkOrders",
                schema: "maint",
                columns: table => new
                {
                    MwoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TriggeredBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlannedStartAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ActualStartAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmWorkOrders", x => x.MwoId);
                    table.ForeignKey(
                        name: "FK_PmWorkOrders_PmPlanTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "maint",
                        principalTable: "PmPlanTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PmChecklistResults",
                schema: "maint",
                columns: table => new
                {
                    ResultId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MwoId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ObservationNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmChecklistResults", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_PmChecklistResults_PmChecklistItems_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "maint",
                        principalTable: "PmChecklistItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PmChecklistResults_PmWorkOrders_MwoId",
                        column: x => x.MwoId,
                        principalSchema: "maint",
                        principalTable: "PmWorkOrders",
                        principalColumn: "MwoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PmChecklistItems_TemplateId",
                schema: "maint",
                table: "PmChecklistItems",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PmChecklistResults_ItemId",
                schema: "maint",
                table: "PmChecklistResults",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PmChecklistResults_MwoId",
                schema: "maint",
                table: "PmChecklistResults",
                column: "MwoId");

            migrationBuilder.CreateIndex(
                name: "IX_PmWorkOrders_TemplateId",
                schema: "maint",
                table: "PmWorkOrders",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineRuntimeAccumulators",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmChecklistResults",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmChecklistItems",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmWorkOrders",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmPlanTemplates",
                schema: "maint");
        }
    }
}
