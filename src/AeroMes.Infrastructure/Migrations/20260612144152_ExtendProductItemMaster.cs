using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductItemMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FixedPurchasePrice",
                schema: "master",
                table: "Products",
                type: "NUMERIC(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityFormula",
                schema: "master",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalStandard",
                schema: "master",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductUoMConversions",
                schema: "master",
                columns: table => new
                {
                    ConversionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToBaseFactor = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUoMConversions", x => x.ConversionId);
                    table.ForeignKey(
                        name: "FK_ProductUoMConversions_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUoMConversions_UnitsOfMeasure_UoMCode",
                        column: x => x.UoMCode,
                        principalSchema: "master",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UoMCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUoMConversions_ProductCode_UoMCode",
                schema: "master",
                table: "ProductUoMConversions",
                columns: new[] { "ProductCode", "UoMCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUoMConversions_UoMCode",
                schema: "master",
                table: "ProductUoMConversions",
                column: "UoMCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductUoMConversions",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "FixedPurchasePrice",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityFormula",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TechnicalStandard",
                schema: "master",
                table: "Products");
        }
    }
}
