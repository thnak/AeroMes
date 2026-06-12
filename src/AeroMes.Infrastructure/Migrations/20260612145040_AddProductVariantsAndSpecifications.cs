using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantsAndSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentProductCode",
                schema: "master",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductSpecifications",
                schema: "master",
                columns: table => new
                {
                    SpecificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpecCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecifications", x => x.SpecificationId);
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentProductCode",
                schema: "master",
                table: "Products",
                column: "ParentProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductCode_SpecCode",
                schema: "master",
                table: "ProductSpecifications",
                columns: new[] { "ProductCode", "SpecCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Products_ParentProductCode",
                schema: "master",
                table: "Products",
                column: "ParentProductCode",
                principalSchema: "master",
                principalTable: "Products",
                principalColumn: "ProductCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_ParentProductCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductSpecifications",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_Products_ParentProductCode",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ParentProductCode",
                schema: "master",
                table: "Products");
        }
    }
}
