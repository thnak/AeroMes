using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendInspectionRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InspectionQuantity",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "DECIMAL(14,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionSubject",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderId",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientDepartment",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatisticalSheetId",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubcontractingOrderId",
                schema: "qual",
                table: "QualityInspectionRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "InspectionQuantity",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "InspectionSubject",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "ProductionOrderId",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "RecipientDepartment",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "StatisticalSheetId",
                schema: "qual",
                table: "QualityInspectionRequests");

            migrationBuilder.DropColumn(
                name: "SubcontractingOrderId",
                schema: "qual",
                table: "QualityInspectionRequests");
        }
    }
}
