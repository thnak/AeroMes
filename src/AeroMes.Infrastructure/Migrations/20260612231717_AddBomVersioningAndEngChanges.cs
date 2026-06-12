using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomVersioningAndEngChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BomHeaders",
                schema: "master",
                columns: table => new
                {
                    BomHeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    BaseQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    EcoReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BomHeaders", x => x.BomHeaderId);
                    table.ForeignKey(
                        name: "FK_BomHeaders_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BomLines",
                schema: "master",
                columns: table => new
                {
                    BomLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomHeaderId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: false),
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScrapFactor = table.Column<decimal>(type: "NUMERIC(5,2)", nullable: false),
                    IsPhantom = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomLines", x => x.BomLineId);
                    table.ForeignKey(
                        name: "FK_BomLines_BomHeaders_BomHeaderId",
                        column: x => x.BomHeaderId,
                        principalSchema: "master",
                        principalTable: "BomHeaders",
                        principalColumn: "BomHeaderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BomLines_Products_ComponentCode",
                        column: x => x.ComponentCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BomLines_UnitsOfMeasure_UoMCode",
                        column: x => x.UoMCode,
                        principalSchema: "master",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UoMCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EngChanges",
                schema: "master",
                columns: table => new
                {
                    EcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EcNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EcType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AffectedProducts = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceEcrNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewBomHeaderId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_EngChanges", x => x.EcId);
                    table.ForeignKey(
                        name: "FK_EngChanges_BomHeaders_NewBomHeaderId",
                        column: x => x.NewBomHeaderId,
                        principalSchema: "master",
                        principalTable: "BomHeaders",
                        principalColumn: "BomHeaderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomHeaders_ProductCode_Status",
                schema: "master",
                table: "BomHeaders",
                columns: new[] { "ProductCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BomHeaders_ProductCode_Version",
                schema: "master",
                table: "BomHeaders",
                columns: new[] { "ProductCode", "Version" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BomLines_BomHeaderId_LineNo",
                schema: "master",
                table: "BomLines",
                columns: new[] { "BomHeaderId", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BomLines_ComponentCode",
                schema: "master",
                table: "BomLines",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_BomLines_UoMCode",
                schema: "master",
                table: "BomLines",
                column: "UoMCode");

            migrationBuilder.CreateIndex(
                name: "IX_EngChanges_EcNumber",
                schema: "master",
                table: "EngChanges",
                column: "EcNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EngChanges_NewBomHeaderId",
                schema: "master",
                table: "EngChanges",
                column: "NewBomHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomLines",
                schema: "master");

            migrationBuilder.DropTable(
                name: "EngChanges",
                schema: "master");

            migrationBuilder.DropTable(
                name: "BomHeaders",
                schema: "master");
        }
    }
}
