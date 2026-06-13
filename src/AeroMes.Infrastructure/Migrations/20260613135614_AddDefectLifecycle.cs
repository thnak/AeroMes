using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "labels");

            migrationBuilder.AddColumn<bool>(
                name: "IsRepairable",
                schema: "qual",
                table: "DefectCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DefectEntries",
                schema: "qual",
                columns: table => new
                {
                    DefectEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<long>(type: "bigint", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    DefectCodeId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RepairableQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ScrapQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectEntries", x => x.DefectEntryId);
                    table.ForeignKey(
                        name: "FK_DefectEntries_DefectCodes_DefectCodeId",
                        column: x => x.DefectCodeId,
                        principalSchema: "qual",
                        principalTable: "DefectCodes",
                        principalColumn: "DefectCodeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabelPrintJobs",
                schema: "labels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrintScope = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrintedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelPrintJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabelTemplates",
                schema: "labels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaperSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BarcodeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BarcodeWidth = table.Column<int>(type: "int", nullable: false),
                    BarcodeHeight = table.Column<int>(type: "int", nullable: false),
                    SelectedFields = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrders",
                schema: "qual",
                columns: table => new
                {
                    RepairOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairOrderNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrders", x => x.RepairOrderId);
                });

            migrationBuilder.CreateTable(
                name: "RepairMaterialLines",
                schema: "qual",
                columns: table => new
                {
                    RepairMaterialLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairOrderId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IssuedQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairMaterialLines", x => x.RepairMaterialLineId);
                    table.ForeignKey(
                        name: "FK_RepairMaterialLines_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalSchema: "qual",
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrderEntries",
                schema: "qual",
                columns: table => new
                {
                    RepairOrderEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairOrderId = table.Column<int>(type: "int", nullable: false),
                    DefectEntryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderEntries", x => x.RepairOrderEntryId);
                    table.ForeignKey(
                        name: "FK_RepairOrderEntries_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalSchema: "qual",
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefectEntries_DefectCodeId",
                schema: "qual",
                table: "DefectEntries",
                column: "DefectCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairMaterialLines_RepairOrderId",
                schema: "qual",
                table: "RepairMaterialLines",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderEntries_RepairOrderId",
                schema: "qual",
                table: "RepairOrderEntries",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_RepairOrderNo",
                schema: "qual",
                table: "RepairOrders",
                column: "RepairOrderNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefectEntries",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "LabelPrintJobs",
                schema: "labels");

            migrationBuilder.DropTable(
                name: "LabelTemplates",
                schema: "labels");

            migrationBuilder.DropTable(
                name: "RepairMaterialLines",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "RepairOrderEntries",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "RepairOrders",
                schema: "qual");

            migrationBuilder.DropColumn(
                name: "IsRepairable",
                schema: "qual",
                table: "DefectCodes");
        }
    }
}
