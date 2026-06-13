using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// NOTE: SQL Server table partitioning on iot.MachineSignalLogs, iot.SignalAgg_1min, and
    /// iot.SignalAgg_1hr is strongly recommended for production environments to improve range-scan
    /// performance and enable efficient partition-level archival. This must be applied manually by
    /// a DBA using CREATE PARTITION FUNCTION / CREATE PARTITION SCHEME / ALTER TABLE … ON
    /// partition_scheme(Timestamp|BucketAt). EF Core does not support partition DDL natively.
    public partial class AddTimeSeriesStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetentionPolicies",
                schema: "iot",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScopeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RawRetentionDays = table.Column<int>(type: "int", nullable: false),
                    Agg1minRetentionDays = table.Column<int>(type: "int", nullable: false),
                    Agg1hrRetentionDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionPolicies", x => x.PolicyId);
                });

            migrationBuilder.CreateTable(
                name: "SignalAgg_1hr",
                schema: "iot",
                columns: table => new
                {
                    BucketId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TagKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BucketAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    SumValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FirstValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalAgg_1hr", x => x.BucketId);
                });

            migrationBuilder.CreateTable(
                name: "SignalAgg_1min",
                schema: "iot",
                columns: table => new
                {
                    BucketId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TagKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BucketAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    SumValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FirstValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalAgg_1min", x => x.BucketId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalAgg_1hr_MachineCode_TagKey_BucketAt",
                schema: "iot",
                table: "SignalAgg_1hr",
                columns: new[] { "MachineCode", "TagKey", "BucketAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalAgg_1min_MachineCode_TagKey_BucketAt",
                schema: "iot",
                table: "SignalAgg_1min",
                columns: new[] { "MachineCode", "TagKey", "BucketAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetentionPolicies",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "SignalAgg_1hr",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "SignalAgg_1min",
                schema: "iot");
        }
    }
}
