using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoldCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Molds",
                schema: "master",
                columns: table => new
                {
                    MoldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoldCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoldName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoldType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Material = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cavities = table.Column<int>(type: "int", nullable: false),
                    MaxShots = table.Column<long>(type: "bigint", nullable: false),
                    CurrentShots = table.Column<long>(type: "bigint", nullable: false),
                    ShotsAtLastPm = table.Column<long>(type: "bigint", nullable: false),
                    PmIntervalShots = table.Column<int>(type: "int", nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentMachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Molds", x => x.MoldId);
                    table.ForeignKey(
                        name: "FK_Molds_Machines_CurrentMachineCode",
                        column: x => x.CurrentMachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MoldMaintenanceLogs",
                schema: "master",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoldId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ShotsAtEvent = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TechnicianId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PartReplaced = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Cost = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: true),
                    NextDueShots = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoldMaintenanceLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_MoldMaintenanceLogs_Molds_MoldId",
                        column: x => x.MoldId,
                        principalSchema: "master",
                        principalTable: "Molds",
                        principalColumn: "MoldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoldProductMappings",
                schema: "master",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoldId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CycleTimeSeconds = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoldProductMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_MoldProductMappings_Molds_MoldId",
                        column: x => x.MoldId,
                        principalSchema: "master",
                        principalTable: "Molds",
                        principalColumn: "MoldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoldProductMappings_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoldMaintenanceLogs_MoldId",
                schema: "master",
                table: "MoldMaintenanceLogs",
                column: "MoldId");

            migrationBuilder.CreateIndex(
                name: "IX_MoldProductMappings_MoldId_ProductCode",
                schema: "master",
                table: "MoldProductMappings",
                columns: new[] { "MoldId", "ProductCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MoldProductMappings_ProductCode",
                schema: "master",
                table: "MoldProductMappings",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_Molds_CurrentMachineCode",
                schema: "master",
                table: "Molds",
                column: "CurrentMachineCode");

            migrationBuilder.CreateIndex(
                name: "IX_Molds_MoldCode",
                schema: "master",
                table: "Molds",
                column: "MoldCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoldMaintenanceLogs",
                schema: "master");

            migrationBuilder.DropTable(
                name: "MoldProductMappings",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Molds",
                schema: "master");
        }
    }
}
