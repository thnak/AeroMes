using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishedProductIntakeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinishedProductIntakeRequests",
                schema: "wms",
                columns: table => new
                {
                    IntakeRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IntakePurpose = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    WarehouseType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: true),
                    RequesterUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_FinishedProductIntakeRequests", x => x.IntakeRequestId);
                    table.ForeignKey(
                        name: "FK_FinishedProductIntakeRequests_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalSchema: "integration",
                        principalTable: "ProductionOrders",
                        principalColumn: "POID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntakeRequestLines",
                schema: "wms",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntakeRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    IsDefective = table.Column<bool>(type: "bit", nullable: false),
                    DefectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActualReceivedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeRequestLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_IntakeRequestLines_FinishedProductIntakeRequests_IntakeRequestId",
                        column: x => x.IntakeRequestId,
                        principalSchema: "wms",
                        principalTable: "FinishedProductIntakeRequests",
                        principalColumn: "IntakeRequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeRequestLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntakeRequestLines_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "master",
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishedProductIntakeRequests_ProductionOrderId",
                schema: "wms",
                table: "FinishedProductIntakeRequests",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedProductIntakeRequests_RequestNumber",
                schema: "wms",
                table: "FinishedProductIntakeRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRequestLines_IntakeRequestId",
                schema: "wms",
                table: "IntakeRequestLines",
                column: "IntakeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRequestLines_ProductCode",
                schema: "wms",
                table: "IntakeRequestLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRequestLines_WarehouseId",
                schema: "wms",
                table: "IntakeRequestLines",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeRequestLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "FinishedProductIntakeRequests",
                schema: "wms");
        }
    }
}
