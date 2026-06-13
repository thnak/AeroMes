using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleCountPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CycleCountPlans",
                schema: "wms",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleCountPlans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "CycleCountLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    CountedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    CountedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CountedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleCountLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_CycleCountLines_Bins_BinId",
                        column: x => x.BinId,
                        principalSchema: "wms",
                        principalTable: "Bins",
                        principalColumn: "BinId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleCountLines_CycleCountPlans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "wms",
                        principalTable: "CycleCountPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CycleCountLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_BinId",
                schema: "wms",
                table: "CycleCountLines",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_PlanId",
                schema: "wms",
                table: "CycleCountLines",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_ProductCode",
                schema: "wms",
                table: "CycleCountLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountPlans_PlanCode",
                schema: "wms",
                table: "CycleCountPlans",
                column: "PlanCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CycleCountLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "CycleCountPlans",
                schema: "wms");
        }
    }
}
