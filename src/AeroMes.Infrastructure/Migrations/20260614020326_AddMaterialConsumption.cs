using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialConsumptions",
                schema: "prod",
                columns: table => new
                {
                    ConsumptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ActualQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialConsumptions", x => x.ConsumptionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_JobId",
                schema: "prod",
                table: "MaterialConsumptions",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialConsumptions",
                schema: "prod");
        }
    }
}
