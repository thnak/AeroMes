using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomTypeSubstituteMaterialDisassemblyBom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BomType",
                schema: "master",
                table: "BomHeaders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "master",
                table: "BomHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BomByProducts",
                schema: "master",
                columns: table => new
                {
                    BomByProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomHeaderId = table.Column<int>(type: "int", nullable: false),
                    ByProductCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    UoMCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomByProducts", x => x.BomByProductId);
                    table.ForeignKey(
                        name: "FK_BomByProducts_BomHeaders_BomHeaderId",
                        column: x => x.BomHeaderId,
                        principalSchema: "master",
                        principalTable: "BomHeaders",
                        principalColumn: "BomHeaderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BomByProducts_Products_ByProductCode",
                        column: x => x.ByProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisassemblyBoms",
                schema: "master",
                columns: table => new
                {
                    DisassemblyBomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BomName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceProductCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    BomType = table.Column<int>(type: "int", nullable: false),
                    LossRatio = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_DisassemblyBoms", x => x.DisassemblyBomId);
                    table.ForeignKey(
                        name: "FK_DisassemblyBoms_Products_SourceProductCode",
                        column: x => x.SourceProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubstituteMaterials",
                schema: "master",
                columns: table => new
                {
                    SubstituteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubstituteCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrimaryMaterialCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    SubstituteMaterialCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ConversionRatio = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_SubstituteMaterials", x => x.SubstituteId);
                    table.ForeignKey(
                        name: "FK_SubstituteMaterials_Products_PrimaryMaterialCode",
                        column: x => x.PrimaryMaterialCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubstituteMaterials_Products_SubstituteMaterialCode",
                        column: x => x.SubstituteMaterialCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisassemblyBomLines",
                schema: "master",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisassemblyBomId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    RecoveryRate = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    FixedQuantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    UoMCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisassemblyBomLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_DisassemblyBomLines_DisassemblyBoms_DisassemblyBomId",
                        column: x => x.DisassemblyBomId,
                        principalSchema: "master",
                        principalTable: "DisassemblyBoms",
                        principalColumn: "DisassemblyBomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisassemblyBomLines_Products_ComponentCode",
                        column: x => x.ComponentCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomByProducts_BomHeaderId",
                schema: "master",
                table: "BomByProducts",
                column: "BomHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BomByProducts_ByProductCode",
                schema: "master",
                table: "BomByProducts",
                column: "ByProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyBomLines_ComponentCode",
                schema: "master",
                table: "DisassemblyBomLines",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyBomLines_DisassemblyBomId",
                schema: "master",
                table: "DisassemblyBomLines",
                column: "DisassemblyBomId");

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyBoms_BomCode",
                schema: "master",
                table: "DisassemblyBoms",
                column: "BomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisassemblyBoms_SourceProductCode",
                schema: "master",
                table: "DisassemblyBoms",
                column: "SourceProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubstituteMaterials_PrimaryMaterialCode",
                schema: "master",
                table: "SubstituteMaterials",
                column: "PrimaryMaterialCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubstituteMaterials_SubstituteCode",
                schema: "master",
                table: "SubstituteMaterials",
                column: "SubstituteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubstituteMaterials_SubstituteMaterialCode",
                schema: "master",
                table: "SubstituteMaterials",
                column: "SubstituteMaterialCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomByProducts",
                schema: "master");

            migrationBuilder.DropTable(
                name: "DisassemblyBomLines",
                schema: "master");

            migrationBuilder.DropTable(
                name: "SubstituteMaterials",
                schema: "master");

            migrationBuilder.DropTable(
                name: "DisassemblyBoms",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "BomType",
                schema: "master",
                table: "BomHeaders");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "master",
                table: "BomHeaders");
        }
    }
}
