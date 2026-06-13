using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockPolicyAndAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockPolicies",
                schema: "wms",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    MinQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    MaxQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    SafetyStockQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ReorderQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_StockPolicies", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_StockPolicies_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockPolicies_StorageLocations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "master",
                        principalTable: "StorageLocations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReplenishmentAlerts",
                schema: "wms",
                columns: table => new
                {
                    AlertId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    CurrentQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LinkedPoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplenishmentAlerts", x => x.AlertId);
                    table.ForeignKey(
                        name: "FK_ReplenishmentAlerts_PurchaseOrders_LinkedPoId",
                        column: x => x.LinkedPoId,
                        principalSchema: "wms",
                        principalTable: "PurchaseOrders",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplenishmentAlerts_StockPolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalSchema: "wms",
                        principalTable: "StockPolicies",
                        principalColumn: "PolicyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentAlerts_LinkedPoId",
                schema: "wms",
                table: "ReplenishmentAlerts",
                column: "LinkedPoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentAlerts_PolicyId",
                schema: "wms",
                table: "ReplenishmentAlerts",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPolicies_LocationId",
                schema: "wms",
                table: "StockPolicies",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPolicies_ProductCode_LocationId",
                schema: "wms",
                table: "StockPolicies",
                columns: new[] { "ProductCode", "LocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplenishmentAlerts",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "StockPolicies",
                schema: "wms");
        }
    }
}
