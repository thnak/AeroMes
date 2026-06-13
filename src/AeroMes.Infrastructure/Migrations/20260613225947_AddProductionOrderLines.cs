using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionOrderMaterialLines",
                schema: "integration",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POID = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StandardQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ActualQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderMaterialLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialLines_ProductionOrders_POID",
                        column: x => x.POID,
                        principalSchema: "integration",
                        principalTable: "ProductionOrders",
                        principalColumn: "POID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderStages",
                schema: "integration",
                columns: table => new
                {
                    StageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkCenterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderStages", x => x.StageId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderStages_ProductionOrders_POID",
                        column: x => x.POID,
                        principalSchema: "integration",
                        principalTable: "ProductionOrders",
                        principalColumn: "POID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialLines_POID",
                schema: "integration",
                table: "ProductionOrderMaterialLines",
                column: "POID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderStages_POID",
                schema: "integration",
                table: "ProductionOrderStages",
                column: "POID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrderMaterialLines",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "ProductionOrderStages",
                schema: "integration");
        }
    }
}
