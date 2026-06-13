using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRmaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnMerchandiseAuthorizations",
                schema: "wms",
                columns: table => new
                {
                    RmaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReturnDirection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    ReturnReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AuthorizedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuthorizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ReturnMerchandiseAuthorizations", x => x.RmaId);
                });

            migrationBuilder.CreateTable(
                name: "RmaLines",
                schema: "wms",
                columns: table => new
                {
                    RmaLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReturnQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NcrId = table.Column<int>(type: "int", nullable: true),
                    StockMovementId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaLines", x => x.RmaLineId);
                    table.ForeignKey(
                        name: "FK_RmaLines_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RmaLines_ReturnMerchandiseAuthorizations_RmaId",
                        column: x => x.RmaId,
                        principalSchema: "wms",
                        principalTable: "ReturnMerchandiseAuthorizations",
                        principalColumn: "RmaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnMerchandiseAuthorizations_RmaCode",
                schema: "wms",
                table: "ReturnMerchandiseAuthorizations",
                column: "RmaCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RmaLines_ProductCode",
                schema: "wms",
                table: "RmaLines",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_RmaLines_RmaId",
                schema: "wms",
                table: "RmaLines",
                column: "RmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RmaLines",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "ReturnMerchandiseAuthorizations",
                schema: "wms");
        }
    }
}
