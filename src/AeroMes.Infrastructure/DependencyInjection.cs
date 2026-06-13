using AeroMes.Application.Interfaces;
using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using AeroMes.Infrastructure.Repositories;
using AeroMes.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroMes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(3))
                // Job, InventoryStock, RoutingStep extend plain Entity (no IsDeleted).
                // Their required FK parents (Machine, StorageLocation, WorkCenter) have soft-delete
                // filters. Deletion of those parents is guarded at the application layer.
                .ConfigureWarnings(w => w.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // master repositories
        services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();
        services.AddScoped<IWorkCalendarRepository, WorkCalendarRepository>();
        services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IBomItemRepository, BomItemRepository>();
        services.AddScoped<IStorageLocationRepository, StorageLocationRepository>();
        services.AddScoped<IRoutingRepository, RoutingRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ICapabilityGroupRepository, CapabilityGroupRepository>();
        services.AddScoped<ICapabilityGroupMemberRepository, CapabilityGroupMemberRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
        services.AddScoped<IOrgUnitRepository, OrgUnitRepository>();
        services.AddScoped<IProductionTeamRepository, ProductionTeamRepository>();
        services.AddScoped<IMoldRepository, MoldRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<IBomHeaderRepository, BomHeaderRepository>();
        services.AddScoped<IEngChangeRepository, EngChangeRepository>();

        // wms repositories
        services.AddScoped<IWarehouseZoneRepository, WarehouseZoneRepository>();
        services.AddScoped<IAisleRepository, AisleRepository>();
        services.AddScoped<IRackRepository, RackRepository>();
        services.AddScoped<IBinRepository, BinRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IGoodsReceiptNoteRepository, GoodsReceiptNoteRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IBeginningInventoryEntryRepository, BeginningInventoryEntryRepository>();
        services.AddScoped<IFactoryWarehouseReceiptRepository, FactoryWarehouseReceiptRepository>();
        services.AddScoped<IFactoryWarehouseExportRepository, FactoryWarehouseExportRepository>();
        services.AddScoped<IMaterialTransferSlipRepository, MaterialTransferSlipRepository>();
        services.AddScoped<IMaterialSupplyRequestRepository, MaterialSupplyRequestRepository>();
        services.AddScoped<IMaterialRequisitionRepository, MaterialRequisitionRepository>();
        services.AddScoped<IFinishedProductIntakeRequestRepository, FinishedProductIntakeRequestRepository>();
        services.AddScoped<ICycleCountPlanRepository, CycleCountPlanRepository>();
        services.AddScoped<IStockPolicyRepository, StockPolicyRepository>();
        services.AddScoped<IReplenishmentAlertRepository, ReplenishmentAlertRepository>();
        services.AddScoped<IStockPolicyEvaluationService, StockPolicyEvaluationService>();
        services.AddScoped<ILotAllocationService, LotAllocationService>();
        services.AddScoped<IRmaRepository, RmaRepository>();
        services.AddScoped<IShipmentOrderRepository, ShipmentOrderRepository>();
        services.AddScoped<IPickListRepository, PickListRepository>();
        services.AddScoped<ICartonRepository, CartonRepository>();

        // integration repositories
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();

        // ERP client + background sync
        services.AddHttpClient("erp").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
        });
        services.AddScoped<IErpClient, HttpErpClient>();
        services.AddHostedService<ErpSyncBackgroundService>();

        // prod repositories
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IProductionLogRepository, ProductionLogRepository>();
        services.AddScoped<IDowntimeLogRepository, DowntimeLogRepository>();
        services.AddScoped<IInventoryStockRepository, InventoryStockRepository>();

        // qual repositories
        services.AddScoped<IDefectCodeRepository, DefectCodeRepository>();
        services.AddScoped<IDefectDetailRepository, DefectDetailRepository>();
        services.AddScoped<IInspectionPlanRepository, InspectionPlanRepository>();
        services.AddScoped<IInspectionCharacteristicRepository, InspectionCharacteristicRepository>();
        services.AddScoped<IInspectionOrderRepository, InspectionOrderRepository>();

        // iot repositories
        services.AddScoped<IAdapterRepository, AdapterRepository>();
        services.AddScoped<ISignalMappingRepository, SignalMappingRepository>();
        services.AddScoped<IMachineStateRuleRepository, MachineStateRuleRepository>();

        // cross-cutting
        services.AddScoped<IModuleStatusRepository, ModuleStatusRepository>();

        services.AddMemoryCache();
        services.AddSingleton<IdempotencyStore>();

        services.AddIdentityCore<ApplicationUser>(opts =>
        {
            opts.Password.RequireDigit = true;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opts.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        // Passkey (WebAuthn) — .NET 10 built-in, no AddPasskeys() extension needed
        services.Configure<IdentityPasskeyOptions>(opts =>
        {
            opts.ServerDomain = configuration["Auth:PasskeyServerDomain"] ?? "localhost";
            var allowedOrigins = configuration.GetSection("Auth:PasskeyAllowedOrigins").Get<string[]>() ?? [];
            opts.ValidateOrigin = ctx => ValueTask.FromResult(
                allowedOrigins.Contains(ctx.Origin, StringComparer.OrdinalIgnoreCase));
        });
        services.AddScoped<IPasskeyHandler<ApplicationUser>, PasskeyHandler<ApplicationUser>>();

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionOverrideRepository, PermissionOverrideRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<ISystemOptionsRepository, SystemOptionsRepository>();
        services.AddScoped<IShiftTemplateRepository, ShiftTemplateRepository>();
        services.AddScoped<IDowntimeReasonCodeRepository, DowntimeReasonCodeRepository>();
        services.AddScoped<IMachineProductConfigRepository, MachineProductConfigRepository>();
        services.AddScoped<IMachineProductParamRepository, MachineProductParamRepository>();
        services.AddScoped<IOperatorCertificationRepository, OperatorCertificationRepository>();
        services.AddScoped<IAlertThresholdRepository, AlertThresholdRepository>();
        services.AddScoped<IWorkOrderAutoRulesRepository, WorkOrderAutoRulesRepository>();

        services.AddSingleton<DbAuditLogger>();
        services.AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<DbAuditLogger>());
        services.AddHostedService(sp => sp.GetRequiredService<DbAuditLogger>());

        return services;
    }
}
