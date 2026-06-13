using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWmsLocationHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wms");

            migrationBuilder.AddColumn<int>(
                name: "BinId",
                schema: "prod",
                table: "InventoryStock",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WarehouseZones",
                schema: "wms",
                columns: table => new
                {
                    ZoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ZoneType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StorageLocationId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    TemperatureZone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_WarehouseZones", x => x.ZoneId);
                });

            migrationBuilder.CreateTable(
                name: "Aisles",
                schema: "wms",
                columns: table => new
                {
                    AisleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    AisleCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PickSequence = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Aisles", x => x.AisleId);
                    table.ForeignKey(
                        name: "FK_Aisles_WarehouseZones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "wms",
                        principalTable: "WarehouseZones",
                        principalColumn: "ZoneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Racks",
                schema: "wms",
                columns: table => new
                {
                    RackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AisleId = table.Column<int>(type: "int", nullable: false),
                    RackCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxWeightKg = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
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
                    table.PrimaryKey("PK_Racks", x => x.RackId);
                    table.ForeignKey(
                        name: "FK_Racks_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalSchema: "wms",
                        principalTable: "Aisles",
                        principalColumn: "AisleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bins",
                schema: "wms",
                columns: table => new
                {
                    BinId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RackId = table.Column<int>(type: "int", nullable: false),
                    BinCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BinLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaxQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    BinType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_Bins", x => x.BinId);
                    table.ForeignKey(
                        name: "FK_Bins_Racks_RackId",
                        column: x => x.RackId,
                        principalSchema: "wms",
                        principalTable: "Racks",
                        principalColumn: "RackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStock_BinId",
                schema: "prod",
                table: "InventoryStock",
                column: "BinId",
                filter: "[BinId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Aisles_ZoneId_AisleCode",
                schema: "wms",
                table: "Aisles",
                columns: new[] { "ZoneId", "AisleCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bins_RackId_BinCode",
                schema: "wms",
                table: "Bins",
                columns: new[] { "RackId", "BinCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Racks_AisleId_RackCode",
                schema: "wms",
                table: "Racks",
                columns: new[] { "AisleId", "RackCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseZones_ZoneCode",
                schema: "wms",
                table: "WarehouseZones",
                column: "ZoneCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStock_Bins_BinId",
                schema: "prod",
                table: "InventoryStock",
                column: "BinId",
                principalSchema: "wms",
                principalTable: "Bins",
                principalColumn: "BinId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStock_Bins_BinId",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropTable(
                name: "Bins",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "Racks",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "Aisles",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "WarehouseZones",
                schema: "wms");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStock_BinId",
                schema: "prod",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "BinId",
                schema: "prod",
                table: "InventoryStock");
        }
    }
}
