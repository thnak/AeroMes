using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialPurchaseRequests",
                schema: "prod",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Requestor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestingUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    ProcurementPurpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceReferenceId = table.Column<int>(type: "int", nullable: true),
                    SalesOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_MaterialPurchaseRequests", x => x.RequestID);
                });

            migrationBuilder.CreateTable(
                name: "MaterialPurchaseRequestLines",
                schema: "prod",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestID = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    CalculatedQty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    Length = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true),
                    Width = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true),
                    Height = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true),
                    Radius = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true),
                    Weight = table.Column<decimal>(type: "DECIMAL(14,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialPurchaseRequestLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_MaterialPurchaseRequestLines_MaterialPurchaseRequests_RequestID",
                        column: x => x.RequestID,
                        principalSchema: "prod",
                        principalTable: "MaterialPurchaseRequests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPurchaseRequestLines_RequestID",
                schema: "prod",
                table: "MaterialPurchaseRequestLines",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPurchaseRequests_RequestNumber",
                schema: "prod",
                table: "MaterialPurchaseRequests",
                column: "RequestNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialPurchaseRequestLines",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "MaterialPurchaseRequests",
                schema: "prod");
        }
    }
}
