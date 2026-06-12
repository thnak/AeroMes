using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "master");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "qual");

            migrationBuilder.EnsureSchema(
                name: "prod");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.EnsureSchema(
                name: "settings");

            migrationBuilder.CreateTable(
                name: "AlertThresholds",
                schema: "master",
                columns: table => new
                {
                    ThresholdId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WarningLevel = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false),
                    CriticalLevel = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AlertThresholds", x => x.ThresholdId);
                });

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
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ForcePasswordChange = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultWorkCenterId = table.Column<int>(type: "int", nullable: true),
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCodes", x => x.DefectCodeID);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeReasonCodes",
                schema: "master",
                columns: table => new
                {
                    ReasonCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SlaMinutes = table.Column<int>(type: "int", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_DowntimeReasonCodes", x => x.ReasonCode);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                schema: "master",
                columns: table => new
                {
                    OperationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Operations", x => x.OperationCode);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "auth",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Resource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(82)", maxLength: 82, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "master",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CategoryCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ProductCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "master",
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "SecurityAuditLog",
                schema: "auth",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActorIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ActorUserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TargetId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLog", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "master",
                columns: table => new
                {
                    SupplierCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierCode);
                });

            migrationBuilder.CreateTable(
                name: "SystemOptions",
                schema: "settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderRetrievalPrevention = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseOrderAutoGenerateProductionPlan = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseOrderDefaultAllocationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaterialManagementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaterialAutoGenerateWarehouseDocs = table.Column<bool>(type: "bit", nullable: false),
                    MaterialTrackExcessAndByProducts = table.Column<bool>(type: "bit", nullable: false),
                    MaterialByProductTracking = table.Column<bool>(type: "bit", nullable: false),
                    MaterialBatchAndExpiryManagement = table.Column<bool>(type: "bit", nullable: false),
                    MaterialDimensionTracking = table.Column<bool>(type: "bit", nullable: false),
                    MaterialUnitConversionEditable = table.Column<bool>(type: "bit", nullable: false),
                    MaterialForecastStockWarning = table.Column<bool>(type: "bit", nullable: false),
                    MaterialDefectRateManagement = table.Column<bool>(type: "bit", nullable: false),
                    CapacityMoldToolingManagement = table.Column<bool>(type: "bit", nullable: false),
                    DispatchAutoGenerateSubAssemblyOrders = table.Column<bool>(type: "bit", nullable: false),
                    DispatchAutoStatusTransition = table.Column<bool>(type: "bit", nullable: false),
                    DispatchSequentialWorkflowEnforcement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DispatchAutoGenerateSupplyRequests = table.Column<bool>(type: "bit", nullable: false),
                    ReportingOverageQuantityAlert = table.Column<bool>(type: "bit", nullable: false),
                    ReportingEditLockAfterQcRequest = table.Column<bool>(type: "bit", nullable: false),
                    ReportingAutoGenerateFromCompletedWorkOrders = table.Column<bool>(type: "bit", nullable: false),
                    ReportingPrintLimitEnforcement = table.Column<bool>(type: "bit", nullable: false),
                    HandoffTrackInterStageTransfers = table.Column<bool>(type: "bit", nullable: false),
                    HandoffAutoConfirmation = table.Column<bool>(type: "bit", nullable: false),
                    AcceptanceAutoGenerateFromProductionReports = table.Column<bool>(type: "bit", nullable: false),
                    AcceptanceQuantityCategorization = table.Column<bool>(type: "bit", nullable: false),
                    QcAutoGenerateRequestsAfterReporting = table.Column<bool>(type: "bit", nullable: false),
                    QcTargetSelection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                schema: "master",
                columns: table => new
                {
                    UoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UoMName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UoMGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.UoMCode);
                });

            migrationBuilder.CreateTable(
                name: "WorkCalendars",
                schema: "master",
                columns: table => new
                {
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalendarName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkCalendars", x => x.WorkCalendarId);
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.WorkCenterID);
                });

            migrationBuilder.CreateTable(
                name: "WorkShifts",
                schema: "master",
                columns: table => new
                {
                    WorkShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    NetMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkShifts", x => x.WorkShiftId);
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
                name: "ApiKeys",
                schema: "auth",
                columns: table => new
                {
                    ApiKeyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KeyPrefix = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    KeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.ApiKeyId);
                    table.ForeignKey(
                        name: "FK_ApiKeys_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CredentialId = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => new { x.UserId, x.CredentialId });
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
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
                name: "RefreshTokens",
                schema: "auth",
                columns: table => new
                {
                    TokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "auth",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "auth",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionOverrides",
                schema: "auth",
                columns: table => new
                {
                    OverrideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    Effect = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionOverrides", x => x.OverrideId);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "auth",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "master",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BarcodePattern = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    BaseUoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchaseUoMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PurchaseToBaseQty = table.Column<decimal>(type: "NUMERIC(18,6)", nullable: false),
                    NetWeight = table.Column<decimal>(type: "NUMERIC(10,4)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "NUMERIC(10,4)", nullable: true),
                    Length = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    Width = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    Height = table.Column<decimal>(type: "NUMERIC(10,2)", nullable: true),
                    LotControlled = table.Column<bool>(type: "bit", nullable: false),
                    SerialControlled = table.Column<bool>(type: "bit", nullable: false),
                    ShelfLifeDays = table.Column<int>(type: "int", nullable: true),
                    ReorderPoint = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    SafetyStock = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    ProcurementType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LifecycleStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CustomerPartNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DrawingNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Revision = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Products", x => x.ProductCode);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "master",
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitsOfMeasure_BaseUoMCode",
                        column: x => x.BaseUoMCode,
                        principalSchema: "master",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UoMCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitsOfMeasure_PurchaseUoMCode",
                        column: x => x.PurchaseUoMCode,
                        principalSchema: "master",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UoMCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarDays",
                schema: "master",
                columns: table => new
                {
                    CalendarDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDays", x => x.CalendarDayId);
                    table.ForeignKey(
                        name: "FK_CalendarDays_WorkCalendars_WorkCalendarId",
                        column: x => x.WorkCalendarId,
                        principalSchema: "master",
                        principalTable: "WorkCalendars",
                        principalColumn: "WorkCalendarId",
                        onDelete: ReferentialAction.Cascade);
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "ShiftTemplates",
                schema: "master",
                columns: table => new
                {
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    ValidDays = table.Column<int>(type: "int", nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ShiftTemplates", x => x.ShiftCode);
                    table.ForeignKey(
                        name: "FK_ShiftTemplates_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_StorageLocations", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK_StorageLocations_WorkCenters_WorkCenterID",
                        column: x => x.WorkCenterID,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderAutoRules",
                schema: "master",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true),
                    AutoStartEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AutoCompleteOnTargetReached = table.Column<bool>(type: "bit", nullable: false),
                    RequireDeleteConfirmToken = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentJobs = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WorkOrderAutoRules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_WorkOrderAutoRules_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "master",
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarExceptions",
                schema: "master",
                columns: table => new
                {
                    CalendarExceptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkCalendarId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkShiftId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarExceptions", x => x.CalendarExceptionId);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_WorkCalendars_WorkCalendarId",
                        column: x => x.WorkCalendarId,
                        principalSchema: "master",
                        principalTable: "WorkCalendars",
                        principalColumn: "WorkCalendarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftBreaks",
                schema: "master",
                columns: table => new
                {
                    ShiftBreakId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkShiftId = table.Column<int>(type: "int", nullable: false),
                    BreakStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakEnd = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftBreaks", x => x.ShiftBreakId);
                    table.ForeignKey(
                        name: "FK_ShiftBreaks_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovedVendorList",
                schema: "master",
                columns: table => new
                {
                    AvlItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    MinOrderQty = table.Column<decimal>(type: "NUMERIC(18,4)", nullable: true),
                    AqlLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovedVendorList", x => x.AvlItemId);
                    table.ForeignKey(
                        name: "FK_ApprovedVendorList_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprovedVendorList_Suppliers_SupplierCode",
                        column: x => x.SupplierCode,
                        principalSchema: "master",
                        principalTable: "Suppliers",
                        principalColumn: "SupplierCode",
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "CalendarShifts",
                schema: "master",
                columns: table => new
                {
                    CalendarShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarDayId = table.Column<int>(type: "int", nullable: false),
                    WorkShiftId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarShifts", x => x.CalendarShiftId);
                    table.ForeignKey(
                        name: "FK_CalendarShifts_CalendarDays_CalendarDayId",
                        column: x => x.CalendarDayId,
                        principalSchema: "master",
                        principalTable: "CalendarDays",
                        principalColumn: "CalendarDayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarShifts_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "master",
                        principalTable: "WorkShifts",
                        principalColumn: "WorkShiftId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DowntimeLogs",
                schema: "prod",
                columns: table => new
                {
                    DowntimeLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                        name: "FK_DowntimeLogs_DowntimeReasonCodes_ReasonCode",
                        column: x => x.ReasonCode,
                        principalSchema: "master",
                        principalTable: "DowntimeReasonCodes",
                        principalColumn: "ReasonCode",
                        onDelete: ReferentialAction.Restrict);
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
                name: "MachineProductConfigs",
                schema: "master",
                columns: table => new
                {
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoutingStepId = table.Column<int>(type: "int", nullable: true),
                    IdealCycleTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TargetThroughputPerHour = table.Column<int>(type: "int", nullable: false),
                    SetupTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineProductConfigs", x => new { x.MachineCode, x.ProductCode });
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_Machines_MachineCode",
                        column: x => x.MachineCode,
                        principalSchema: "master",
                        principalTable: "Machines",
                        principalColumn: "MachineCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalSchema: "master",
                        principalTable: "Products",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineProductConfigs_RoutingSteps_RoutingStepId",
                        column: x => x.RoutingStepId,
                        principalSchema: "master",
                        principalTable: "RoutingSteps",
                        principalColumn: "RoutingStepID",
                        onDelete: ReferentialAction.Restrict);
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                        name: "FK_Jobs_ShiftTemplates_ShiftCode",
                        column: x => x.ShiftCode,
                        principalSchema: "master",
                        principalTable: "ShiftTemplates",
                        principalColumn: "ShiftCode",
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
                name: "IX_ApiKeys_KeyHash",
                schema: "auth",
                table: "ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_OwnerUserId",
                schema: "auth",
                table: "ApiKeys",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovedVendorList_ProductCode",
                schema: "master",
                table: "ApprovedVendorList",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovedVendorList_SupplierCode_ProductCode",
                schema: "master",
                table: "ApprovedVendorList",
                columns: new[] { "SupplierCode", "ProductCode" },
                unique: true);

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
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDays_WorkCalendarId_DayOfWeek",
                schema: "master",
                table: "CalendarDays",
                columns: new[] { "WorkCalendarId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_WorkCalendarId_Date",
                schema: "master",
                table: "CalendarExceptions",
                columns: new[] { "WorkCalendarId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_WorkShiftId",
                schema: "master",
                table: "CalendarExceptions",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShifts_CalendarDayId",
                schema: "master",
                table: "CalendarShifts",
                column: "CalendarDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShifts_WorkShiftId",
                schema: "master",
                table: "CalendarShifts",
                column: "WorkShiftId");

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
                name: "IX_DowntimeLogs_ReasonCode",
                schema: "prod",
                table: "DowntimeLogs",
                column: "ReasonCode");

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
                name: "IX_Jobs_ShiftCode",
                schema: "prod",
                table: "Jobs",
                column: "ShiftCode");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WOID",
                schema: "prod",
                table: "Jobs",
                column: "WOID");

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductConfigs_ProductCode",
                schema: "master",
                table: "MachineProductConfigs",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductConfigs_RoutingStepId",
                schema: "master",
                table: "MachineProductConfigs",
                column: "RoutingStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkCenterID",
                schema: "master",
                table: "Machines",
                column: "WorkCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionCode",
                schema: "auth",
                table: "Permissions",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Action",
                schema: "auth",
                table: "Permissions",
                columns: new[] { "Resource", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryCode",
                schema: "master",
                table: "ProductCategories",
                column: "CategoryCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentId",
                schema: "master",
                table: "ProductCategories",
                column: "ParentId");

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
                name: "IX_Products_BaseUoMCode",
                schema: "master",
                table: "Products",
                column: "BaseUoMCode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "master",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurchaseUoMCode",
                schema: "master",
                table: "Products",
                column: "PurchaseUoMCode");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId",
                schema: "auth",
                table: "RefreshTokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "auth",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "auth",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "auth",
                table: "RolePermissions",
                column: "PermissionId");

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
                unique: true,
                filter: "[IsDeleted] = 0");

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
                name: "IX_SecurityAuditLog_ActorId_OccurredAt",
                schema: "auth",
                table: "SecurityAuditLog",
                columns: new[] { "ActorId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLog_EventType_OccurredAt",
                schema: "auth",
                table: "SecurityAuditLog",
                columns: new[] { "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLog_TargetType_TargetId_OccurredAt",
                schema: "auth",
                table: "SecurityAuditLog",
                columns: new[] { "TargetType", "TargetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftBreaks_WorkShiftId",
                schema: "master",
                table: "ShiftBreaks",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTemplates_WorkCenterId",
                schema: "master",
                table: "ShiftTemplates",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_LocationCode",
                schema: "master",
                table: "StorageLocations",
                column: "LocationCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_WorkCenterID",
                schema: "master",
                table: "StorageLocations",
                column: "WorkCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_PermissionId",
                schema: "auth",
                table: "UserPermissionOverrides",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_UserId_PermissionId",
                schema: "auth",
                table: "UserPermissionOverrides",
                columns: new[] { "UserId", "PermissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkCalendars_CalendarCode",
                schema: "master",
                table: "WorkCalendars",
                column: "CalendarCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_WorkCenterCode",
                schema: "master",
                table: "WorkCenters",
                column: "WorkCenterCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAutoRules_WorkCenterId",
                schema: "master",
                table: "WorkOrderAutoRules",
                column: "WorkCenterId",
                unique: true,
                filter: "[WorkCenterId] IS NOT NULL AND [IsDeleted] = 0");

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

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_ShiftCode",
                schema: "master",
                table: "WorkShifts",
                column: "ShiftCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertThresholds",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ApiKeys",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ApprovedVendorList",
                schema: "master");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BOM",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CalendarExceptions",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CalendarShifts",
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
                name: "MachineProductConfigs",
                schema: "master");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "SecurityAuditLog",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ShiftBreaks",
                schema: "master");

            migrationBuilder.DropTable(
                name: "SystemOptions",
                schema: "settings");

            migrationBuilder.DropTable(
                name: "UserPermissionOverrides",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "WorkOrderAutoRules",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CalendarDays",
                schema: "master");

            migrationBuilder.DropTable(
                name: "DefectCodes",
                schema: "qual");

            migrationBuilder.DropTable(
                name: "ProductionLogs",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "DowntimeReasonCodes",
                schema: "master");

            migrationBuilder.DropTable(
                name: "StorageLocations",
                schema: "master");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "WorkShifts",
                schema: "master");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "WorkCalendars",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "prod");

            migrationBuilder.DropTable(
                name: "Machines",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ShiftTemplates",
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

            migrationBuilder.DropTable(
                name: "ProductCategories",
                schema: "master");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure",
                schema: "master");
        }
    }
}
