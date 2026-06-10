using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductUnit",
                schema: "master",
                table: "Products",
                newName: "LifecycleStatus");

            migrationBuilder.RenameColumn(
                name: "IsFinishedGood",
                schema: "master",
                table: "Products",
                newName: "SerialControlled");

            migrationBuilder.AlterColumn<string>(
                name: "BarcodePattern",
                schema: "master",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseUoMCode",
                schema: "master",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                schema: "master",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPartNo",
                schema: "master",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrawingNo",
                schema: "master",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "master",
                table: "Products",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                schema: "master",
                table: "Products",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                schema: "master",
                table: "Products",
                type: "NUMERIC(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                schema: "master",
                table: "Products",
                type: "NUMERIC(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "master",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                schema: "master",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                schema: "master",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                schema: "master",
                table: "Products",
                type: "NUMERIC(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LotControlled",
                schema: "master",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                schema: "master",
                table: "Products",
                type: "NUMERIC(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementType",
                schema: "master",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseToBaseQty",
                schema: "master",
                table: "Products",
                type: "NUMERIC(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseUoMCode",
                schema: "master",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderPoint",
                schema: "master",
                table: "Products",
                type: "NUMERIC(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Revision",
                schema: "master",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SafetyStock",
                schema: "master",
                table: "Products",
                type: "NUMERIC(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                schema: "master",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                schema: "master",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                schema: "master",
                table: "Products",
                type: "NUMERIC(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "master",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CategoryCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_ProductCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "master",
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                schema: "master",
                columns: table => new
                {
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UoMName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UoMGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.UoMCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BaseUoMCode",
                schema: "master",
                table: "Products",
                column: "BaseUoMCode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "master",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurchaseUoMCode",
                schema: "master",
                table: "Products",
                column: "PurchaseUoMCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryCode",
                schema: "master",
                table: "ProductCategories",
                column: "CategoryCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentId",
                schema: "master",
                table: "ProductCategories",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                schema: "master",
                table: "Products",
                column: "CategoryId",
                principalSchema: "master",
                principalTable: "ProductCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_BaseUoMCode",
                schema: "master",
                table: "Products",
                column: "BaseUoMCode",
                principalSchema: "master",
                principalTable: "UnitsOfMeasure",
                principalColumn: "UoMCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_PurchaseUoMCode",
                schema: "master",
                table: "Products",
                column: "PurchaseUoMCode",
                principalSchema: "master",
                principalTable: "UnitsOfMeasure",
                principalColumn: "UoMCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                schema: "master",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_BaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_PurchaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories",
                schema: "master");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_Products_BaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "master",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PurchaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomerPartNo",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DrawingNo",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ItemType",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LotControlled",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProcurementType",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseToBaseQty",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseUoMCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Revision",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SafetyStock",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Width",
                schema: "master",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SerialControlled",
                schema: "master",
                table: "Products",
                newName: "IsFinishedGood");

            migrationBuilder.RenameColumn(
                name: "LifecycleStatus",
                schema: "master",
                table: "Products",
                newName: "ProductUnit");

            migrationBuilder.AlterColumn<string>(
                name: "BarcodePattern",
                schema: "master",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
