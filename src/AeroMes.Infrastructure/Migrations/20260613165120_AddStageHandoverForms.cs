using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStageHandoverForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StageHandoverForms",
                schema: "prod",
                columns: table => new
                {
                    FormID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FormType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FromWorkOrderID = table.Column<int>(type: "int", nullable: false),
                    FromRoutingStepID = table.Column<int>(type: "int", nullable: false),
                    ToWorkOrderID = table.Column<int>(type: "int", nullable: false),
                    ToRoutingStepID = table.Column<int>(type: "int", nullable: false),
                    HandoverDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageHandoverForms", x => x.FormID);
                });

            migrationBuilder.CreateTable(
                name: "HandoverLineItems",
                schema: "prod",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Qty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoverLineItems", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_HandoverLineItems_StageHandoverForms_FormID",
                        column: x => x.FormID,
                        principalSchema: "prod",
                        principalTable: "StageHandoverForms",
                        principalColumn: "FormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HandoverLineItems_FormID",
                schema: "prod",
                table: "HandoverLineItems",
                column: "FormID");

            migrationBuilder.CreateIndex(
                name: "IX_StageHandoverForms_FormNumber",
                schema: "prod",
                table: "StageHandoverForms",
                column: "FormNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageHandoverForms_FromWorkOrderID",
                schema: "prod",
                table: "StageHandoverForms",
                column: "FromWorkOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_StageHandoverForms_ToWorkOrderID",
                schema: "prod",
                table: "StageHandoverForms",
                column: "ToWorkOrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandoverLineItems",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "StageHandoverForms",
                schema: "prod");
        }
    }
}
