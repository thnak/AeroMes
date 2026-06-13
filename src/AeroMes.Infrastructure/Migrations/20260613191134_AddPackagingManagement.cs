using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackagingBoms",
                schema: "prod",
                columns: table => new
                {
                    PackagingBomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingBoms", x => x.PackagingBomID);
                });

            migrationBuilder.CreateTable(
                name: "PackagingOrders",
                schema: "prod",
                columns: table => new
                {
                    PackagingOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    PackagingBomID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentificationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlannedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    PackagedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingOrders", x => x.PackagingOrderID);
                });

            migrationBuilder.CreateTable(
                name: "PackagingBomLines",
                schema: "prod",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingBomID = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    UnitCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingBomLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_PackagingBomLines_PackagingBoms_PackagingBomID",
                        column: x => x.PackagingBomID,
                        principalSchema: "prod",
                        principalTable: "PackagingBoms",
                        principalColumn: "PackagingBomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackagingLabels",
                schema: "prod",
                columns: table => new
                {
                    LabelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingOrderID = table.Column<int>(type: "int", nullable: false),
                    LabelData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingLabels", x => x.LabelID);
                    table.ForeignKey(
                        name: "FK_PackagingLabels_PackagingOrders_PackagingOrderID",
                        column: x => x.PackagingOrderID,
                        principalSchema: "prod",
                        principalTable: "PackagingOrders",
                        principalColumn: "PackagingOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagingBomLines_PackagingBomID",
                schema: "prod",
                table: "PackagingBomLines",
                column: "PackagingBomID");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingLabels_PackagingOrderID",
                schema: "prod",
                table: "PackagingLabels",
                column: "PackagingOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_WOID",
                schema: "prod",
                table: "PackagingOrders",
                column: "WOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagingBomLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "PackagingLabels",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "PackagingBoms",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "PackagingOrders",
                schema: "prod");
        }
    }
}
