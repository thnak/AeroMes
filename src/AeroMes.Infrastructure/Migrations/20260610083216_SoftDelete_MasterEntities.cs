using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete_MasterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters");

            migrationBuilder.DropIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropIndex(
                name: "IX_Routings_RoutingCode",
                schema: "master",
                table: "Routings");

            migrationBuilder.DropIndex(
                name: "IX_BOM_ParentProductCode_ChildProductCode",
                schema: "master",
                table: "BOM");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "prod",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "prod",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "prod",
                table: "WorkOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "WorkCenters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "WorkCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "WorkCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "master",
                table: "StorageLocations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "master",
                table: "StorageLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "StorageLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "StorageLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "StorageLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "master",
                table: "StorageLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "master",
                table: "StorageLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "Routings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "Routings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "Routings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "master",
                table: "Operations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "master",
                table: "Operations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "Operations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "Operations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "Operations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "master",
                table: "Operations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "master",
                table: "Operations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "Machines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "Machines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "qual",
                table: "DefectCodes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "qual",
                table: "DefectCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "qual",
                table: "DefectCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "master",
                table: "BOM",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "master",
                table: "BOM",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "master",
                table: "BOM",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters",
                column: "WorkCenterCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations",
                column: "LocationCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Routings_RoutingCode",
                schema: "master",
                table: "Routings",
                column: "RoutingCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BOM_ParentProductCode_ChildProductCode",
                schema: "master",
                table: "BOM",
                columns: new[] { "ParentProductCode", "ChildProductCode" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters");

            migrationBuilder.DropIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropIndex(
                name: "IX_Routings_RoutingCode",
                schema: "master",
                table: "Routings");

            migrationBuilder.DropIndex(
                name: "IX_BOM_ParentProductCode_ChildProductCode",
                schema: "master",
                table: "BOM");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "prod",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "prod",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "prod",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "WorkCenters");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "WorkCenters");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "WorkCenters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "master",
                table: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "master",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "qual",
                table: "DefectCodes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "master",
                table: "BOM");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "master",
                table: "BOM");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "master",
                table: "BOM");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters",
                column: "WorkCenterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations",
                column: "LocationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routings_RoutingCode",
                schema: "master",
                table: "Routings",
                column: "RoutingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BOM_ParentProductCode_ChildProductCode",
                schema: "master",
                table: "BOM",
                columns: new[] { "ParentProductCode", "ChildProductCode" },
                unique: true);
        }
    }
}
