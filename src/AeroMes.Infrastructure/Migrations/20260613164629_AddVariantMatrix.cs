using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductFamilies",
                schema: "master",
                columns: table => new
                {
                    FamilyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaseProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'GENERAL'"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFamilies", x => x.FamilyCode);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                schema: "master",
                columns: table => new
                {
                    VariantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VariantKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VariantAttributes = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false, defaultValueSql: "'{}'"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.VariantID);
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProductFamilies_FamilyCode",
                        column: x => x.FamilyCode,
                        principalSchema: "master",
                        principalTable: "ProductFamilies",
                        principalColumn: "FamilyCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VariantDimensions",
                schema: "master",
                columns: table => new
                {
                    DimensionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DimensionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantDimensions", x => x.DimensionID);
                    table.ForeignKey(
                        name: "FK_VariantDimensions_ProductFamilies_FamilyCode",
                        column: x => x.FamilyCode,
                        principalSchema: "master",
                        principalTable: "ProductFamilies",
                        principalColumn: "FamilyCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantDimensionValues",
                schema: "master",
                columns: table => new
                {
                    ValueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionID = table.Column<int>(type: "int", nullable: false),
                    ValueCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ValueLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantDimensionValues", x => x.ValueID);
                    table.ForeignKey(
                        name: "FK_VariantDimensionValues_VariantDimensions_DimensionID",
                        column: x => x.DimensionID,
                        principalSchema: "master",
                        principalTable: "VariantDimensions",
                        principalColumn: "DimensionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_FamilyCode_VariantKey",
                schema: "master",
                table: "ProductVariants",
                columns: new[] { "FamilyCode", "VariantKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductCode",
                schema: "master",
                table: "ProductVariants",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_VariantDimensions_FamilyCode_DimensionName",
                schema: "master",
                table: "VariantDimensions",
                columns: new[] { "FamilyCode", "DimensionName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantDimensionValues_DimensionID_ValueCode",
                schema: "master",
                table: "VariantDimensionValues",
                columns: new[] { "DimensionID", "ValueCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVariants",
                schema: "master");

            migrationBuilder.DropTable(
                name: "VariantDimensionValues",
                schema: "master");

            migrationBuilder.DropTable(
                name: "VariantDimensions",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ProductFamilies",
                schema: "master");
        }
    }
}
