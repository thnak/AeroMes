using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityInspectionVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityInspectionVouchers",
                schema: "qual",
                columns: table => new
                {
                    VoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoucherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InspectionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LinkedRequestId = table.Column<int>(type: "int", nullable: true),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: true),
                    SampleQuantity = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    PassingSamples = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    FailingSamples = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false),
                    Conclusion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NotStarted"),
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
                    table.PrimaryKey("PK_QualityInspectionVouchers", x => x.VoucherID);
                });

            migrationBuilder.CreateTable(
                name: "VoucherDefectDetails",
                schema: "qual",
                columns: table => new
                {
                    DetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherID = table.Column<int>(type: "int", nullable: false),
                    DefectCodeId = table.Column<int>(type: "int", nullable: false),
                    DefectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(12,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherDefectDetails", x => x.DetailID);
                    table.ForeignKey(
                        name: "FK_VoucherDefectDetails_QualityInspectionVouchers_VoucherID",
                        column: x => x.VoucherID,
                        principalSchema: "qual",
                        principalTable: "QualityInspectionVouchers",
                        principalColumn: "VoucherID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspectionVouchers_VoucherNumber",
                schema: "qual",
                table: "QualityInspectionVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDefectDetails_VoucherID",
                schema: "qual",
                table: "VoucherDefectDetails",
                column: "VoucherID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherDefectDetails",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "QualityInspectionVouchers",
                schema: "qual");
        }
    }
}
