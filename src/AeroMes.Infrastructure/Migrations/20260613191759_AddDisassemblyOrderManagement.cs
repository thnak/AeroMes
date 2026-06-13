using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisassemblyOrderManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisassemblyOrders",
                schema: "prod",
                columns: table => new
                {
                    DisassemblyOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: true),
                    SourceProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisassemblyBomId = table.Column<int>(type: "int", nullable: false),
                    SourceQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisassemblyOrders", x => x.DisassemblyOrderID);
                });

            migrationBuilder.CreateTable(
                name: "DisassemblyRecoveredLines",
                schema: "prod",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisassemblyOrderID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpectedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ActualQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisassemblyRecoveredLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_DisassemblyRecoveredLines_DisassemblyOrders_DisassemblyOrderID",
                        column: x => x.DisassemblyOrderID,
                        principalSchema: "prod",
                        principalTable: "DisassemblyOrders",
                        principalColumn: "DisassemblyOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyOrders_OrderCode",
                schema: "prod",
                table: "DisassemblyOrders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyOrders_Status",
                schema: "prod",
                table: "DisassemblyOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyRecoveredLines_DisassemblyOrderID",
                schema: "prod",
                table: "DisassemblyRecoveredLines",
                column: "DisassemblyOrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisassemblyRecoveredLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "DisassemblyOrders",
                schema: "prod");
        }
    }
}
