using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                schema: "integration",
                table: "SalesOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "master",
                columns: table => new
                {
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreditTermsDays = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Customers", x => x.CustomerCode);
                });

            migrationBuilder.CreateTable(
                name: "CustomerPartNumbers",
                schema: "master",
                columns: table => new
                {
                    CustomerPartNumberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerPartNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DrawingReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Revision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPartNumbers", x => x.CustomerPartNumberId);
                    table.ForeignKey(
                        name: "FK_CustomerPartNumbers_Customers_CustomerCode",
                        column: x => x.CustomerCode,
                        principalSchema: "master",
                        principalTable: "Customers",
                        principalColumn: "CustomerCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerPartNumbers_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerQualitySpecs",
                schema: "master",
                columns: table => new
                {
                    CustomerQualitySpecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AqlLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InspectionLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxDefectsPpm = table.Column<int>(type: "int", nullable: true),
                    SpecialRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerQualitySpecs", x => x.CustomerQualitySpecId);
                    table.ForeignKey(
                        name: "FK_CustomerQualitySpecs_Customers_CustomerCode",
                        column: x => x.CustomerCode,
                        principalSchema: "master",
                        principalTable: "Customers",
                        principalColumn: "CustomerCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerQualitySpecs_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerCode",
                schema: "integration",
                table: "SalesOrders",
                column: "CustomerCode");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPartNumbers_CustomerCode_CustomerPartNo",
                schema: "master",
                table: "CustomerPartNumbers",
                columns: new[] { "CustomerCode", "CustomerPartNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPartNumbers_ProductCode",
                schema: "master",
                table: "CustomerPartNumbers",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQualitySpecs_CustomerCode_ProductCode",
                schema: "master",
                table: "CustomerQualitySpecs",
                columns: new[] { "CustomerCode", "ProductCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQualitySpecs_ProductCode",
                schema: "master",
                table: "CustomerQualitySpecs",
                column: "ProductCode");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerCode",
                schema: "integration",
                table: "SalesOrders",
                column: "CustomerCode",
                principalSchema: "master",
                principalTable: "Customers",
                principalColumn: "CustomerCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerCode",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropTable(
                name: "CustomerPartNumbers",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CustomerQualitySpecs",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_CustomerCode",
                schema: "integration",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                schema: "integration",
                table: "SalesOrders");
        }
    }
}
