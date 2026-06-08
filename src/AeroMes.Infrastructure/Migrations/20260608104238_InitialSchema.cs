using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "master");

            migrationBuilder.EnsureSchema(
                name: "qual");

            migrationBuilder.EnsureSchema(
                name: "prod");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefectCodes",
                schema: "qual",
                columns: table => new
                {
                    DefectCodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefectName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefectCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCodes", x => x.DefectCodeID);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                schema: "master",
                columns: table => new
                {
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.OperationCode);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "master",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BarcodePattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFinishedGood = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductCode);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                schema: "integration",
                columns: table => new
                {
                    SOID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SOCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.SOID);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                schema: "master",
                columns: table => new
                {
                    WorkCenterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCenterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkCenterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.WorkCenterID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOM",
                schema: "master",
                columns: table => new
                {
                    BomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChildProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    ScrapFactor = table.Column<decimal>(type: "NUMERIC(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOM", x => x.BomID);
                    table.ForeignKey(
                        name: "FK_BOM_Products_ChildProductCode",
                        column: x => x.ChildProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BOM_Products_ParentProductCode",
                        column: x => x.ParentProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routings",
                schema: "master",
                columns: table => new
                {
                    RoutingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoutingName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routings", x => x.RoutingID);
                    table.ForeignKey(
                        name: "FK_Routings_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                schema: "integration",
                columns: table => new
                {
                    POID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SOID = table.Column<int>(type: "int", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.POID);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_SalesOrders_SOID",
                        column: x => x.SOID,
                        principalSchema: "integration",
                        principalTable: "SalesOrders",
                        principalColumn: "SOID");
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                schema: "master",
                columns: table => new
                {
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineCode);
                    table.ForeignKey(
                        name: "FK_Machines_WorkCenters_WorkCenterID",
                        column: x => x.WorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                schema: "master",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK_StorageLocations_WorkCenters_WorkCenterID",
                        column: x => x.WorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID");
                });

            migrationBuilder.CreateTable(
                name: "RoutingSteps",
                schema: "master",
                columns: table => new
                {
                    RoutingStepID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingID = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DefaultWorkCenterID = table.Column<int>(type: "int", nullable: false),
                    StandardCycleTime = table.Column<double>(type: "float", nullable: false),
                    IsQcRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingSteps", x => x.RoutingStepID);
                    table.ForeignKey(
                        name: "FK_RoutingSteps_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalSchema: "master",
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutingSteps_Routings_RoutingID",
                        column: x => x.RoutingID,
                        principalSchema: "master",
                        principalTable: "Routings",
                        principalColumn: "RoutingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutingSteps_WorkCenters_DefaultWorkCenterID",
                        column: x => x.DefaultWorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeLogs",
                schema: "prod",
                columns: table => new
                {
                    DowntimeLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeLogs", x => x.DowntimeLogID);
                    table.ForeignKey(
                        name: "FK_DowntimeLogs_Machines_MachineCode",
                        column: x => x.MachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStock",
                schema: "prod",
                columns: table => new
                {
                    StockID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStock", x => x.StockID);
                    table.ForeignKey(
                        name: "FK_InventoryStock_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryStock_StorageLocations_LocationID",
                        column: x => x.LocationID,
                        principalSchema: "master",
                        principalTable: "StorageLocations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                schema: "prod",
                columns: table => new
                {
                    WOID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    POID = table.Column<int>(type: "int", nullable: false),
                    RoutingStepID = table.Column<int>(type: "int", nullable: false),
                    WorkCenterID = table.Column<int>(type: "int", nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualQtyOK = table.Column<int>(type: "int", nullable: false),
                    ActualQtyNG = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.WOID);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProductionOrders_POID",
                        column: x => x.POID,
                        principalSchema: "integration",
                        principalTable: "ProductionOrders",
                        principalColumn: "POID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_RoutingSteps_RoutingStepID",
                        column: x => x.RoutingStepID,
                        principalSchema: "master",
                        principalTable: "RoutingSteps",
                        principalColumn: "RoutingStepID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_WorkCenters_WorkCenterID",
                        column: x => x.WorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "prod",
                columns: table => new
                {
                    JobID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WOID = table.Column<int>(type: "int", nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OperatorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobID);
                    table.ForeignKey(
                        name: "FK_Jobs_Machines_MachineCode",
                        column: x => x.MachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_WorkOrders_WOID",
                        column: x => x.WOID,
                        principalSchema: "prod",
                        principalTable: "WorkOrders",
                        principalColumn: "WOID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLogs",
                schema: "prod",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QtyOK = table.Column<int>(type: "int", nullable: false),
                    QtyNG = table.Column<int>(type: "int", nullable: false),
                    DeviceIP = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_ProductionLogs_Jobs_JobID",
                        column: x => x.JobID,
                        principalSchema: "prod",
                        principalTable: "Jobs",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefectDetails",
                schema: "qual",
                columns: table => new
                {
                    DefectDetailID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogID = table.Column<long>(type: "bigint", nullable: false),
                    DefectCodeID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectDetails", x => x.DefectDetailID);
                    table.ForeignKey(
                        name: "FK_DefectDetails_DefectCodes_DefectCodeID",
                        column: x => x.DefectCodeID,
                        principalSchema: "qual",
                        principalTable: "DefectCodes",
                        principalColumn: "DefectCodeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefectDetails_ProductionLogs_LogID",
                        column: x => x.LogID,
                        principalSchema: "prod",
                        principalTable: "ProductionLogs",
                        principalColumn: "LogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BOM_ChildProductCode",
                schema: "master",
                table: "BOM",
                column: "ChildProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_BOM_ParentProductCode_ChildProductCode",
                schema: "master",
                table: "BOM",
                columns: new[] { "ParentProductCode", "ChildProductCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefectCodes_Code",
                schema: "qual",
                table: "DefectCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefectDetails_DefectCodeID",
                schema: "qual",
                table: "DefectDetails",
                column: "DefectCodeID");

            migrationBuilder.CreateIndex(
                name: "IX_DefectDetails_LogID",
                schema: "qual",
                table: "DefectDetails",
                column: "LogID");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeLogs_MachineCode",
                schema: "prod",
                table: "DowntimeLogs",
                column: "MachineCode");

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeLogs_StartTime",
                schema: "prod",
                table: "DowntimeLogs",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStock_LocationID_ProductCode_LotNumber",
                schema: "prod",
                table: "InventoryStock",
                columns: new[] { "LocationID", "ProductCode", "LotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStock_ProductCode_LotNumber",
                schema: "prod",
                table: "InventoryStock",
                columns: new[] { "ProductCode", "LotNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MachineCode",
                schema: "prod",
                table: "Jobs",
                column: "MachineCode");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WOID",
                schema: "prod",
                table: "Jobs",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkCenterID",
                schema: "master",
                table: "Machines",
                column: "WorkCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_IdempotencyKey",
                schema: "prod",
                table: "ProductionLogs",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_JobID",
                schema: "prod",
                table: "ProductionLogs",
                column: "JobID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_Timestamp",
                schema: "prod",
                table: "ProductionLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_POCode",
                schema: "integration",
                table: "ProductionOrders",
                column: "POCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductCode",
                schema: "integration",
                table: "ProductionOrders",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_SOID",
                schema: "integration",
                table: "ProductionOrders",
                column: "SOID");

            migrationBuilder.CreateIndex(
                name: "IX_Routings_ProductCode",
                schema: "master",
                table: "Routings",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_Routings_RoutingCode",
                schema: "master",
                table: "Routings",
                column: "RoutingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingSteps_DefaultWorkCenterID",
                schema: "master",
                table: "RoutingSteps",
                column: "DefaultWorkCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingSteps_OperationCode",
                schema: "master",
                table: "RoutingSteps",
                column: "OperationCode");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingSteps_RoutingID_StepNumber",
                schema: "master",
                table: "RoutingSteps",
                columns: new[] { "RoutingID", "StepNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SOCode",
                schema: "integration",
                table: "SalesOrders",
                column: "SOCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations",
                column: "LocationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_WorkCenterID",
                schema: "master",
                table: "StorageLocations",
                column: "WorkCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters",
                column: "WorkCenterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_POID",
                schema: "prod",
                table: "WorkOrders",
                column: "POID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_RoutingStepID",
                schema: "prod",
                table: "WorkOrders",
                column: "RoutingStepID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WOCode",
                schema: "prod",
                table: "WorkOrders",
                column: "WOCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkCenterID",
                schema: "prod",
                table: "WorkOrders",
                column: "WorkCenterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BOM",
                schema: "master");

            migrationBuilder.DropTable(
                name: "DefectDetails",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "DowntimeLogs",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "InventoryStock",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DefectCodes",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "ProductionLogs",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "StorageLocations",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "Machines",
                schema: "master");

            migrationBuilder.DropTable(
                name: "WorkOrders",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "ProductionOrders",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "RoutingSteps",
                schema: "master");

            migrationBuilder.DropTable(
                name: "SalesOrders",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "Operations",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Routings",
                schema: "master");

            migrationBuilder.DropTable(
                name: "WorkCenters",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "master");
        }
    }
}
