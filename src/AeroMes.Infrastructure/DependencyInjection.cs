using AeroMes.Application.Interfaces;
using AeroMes.Application.Traceability.Services;
using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Domain.Iot.Events;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Maintenance.Repositories;
using AeroMes.Domain.Energy.Repositories;
using AeroMes.Domain.Lab.Repositories;
using AeroMes.Domain.Labels.Repositories;
using AeroMes.Domain.Reminders.Repositories;
using AeroMes.Application.Labeling.Services;
using AeroMes.Application.Storage;
using AeroMes.Domain.Storage.Repositories;
using AeroMes.Infrastructure.Labeling;
using AeroMes.Domain.Sop.Repositories;
using AeroMes.Domain.Rules.Repositories;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using AeroMes.Infrastructure.Iot;
using AeroMes.Infrastructure.Iot.Modbus;
using AeroMes.Infrastructure.Iot.Mqtt;
using AeroMes.Infrastructure.Iot.OpcUa;
using AeroMes.Infrastructure.Repositories;
using AeroMes.Infrastructure.Lab;
using AeroMes.Infrastructure.Labels;
using AeroMes.Infrastructure.Reminders;
using AeroMes.Infrastructure.Storage;
using AeroMes.Infrastructure.Rules;
using AeroMes.Infrastructure.Sop;
using AeroMes.Infrastructure.Services;
using LiteBus.Events.Abstractions;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        services.AddScoped<ISubstituteMaterialRepository, SubstituteMaterialRepository>();
        services.AddScoped<IDisassemblyBomRepository, DisassemblyBomRepository>();
        services.AddScoped<IEngChangeRepository, EngChangeRepository>();
        services.AddScoped<IProductFamilyRepository, ProductFamilyRepository>();
        services.AddScoped<IStageHandoverRepository, StageHandoverRepository>();

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
        services.AddScoped<IMultiProductionOrderRepository, MultiProductionOrderRepository>();
        services.AddScoped<IProductionOrderProgressRepository, ProductionOrderProgressRepository>();
        services.AddScoped<IProductionStatisticsSheetRepository, ProductionStatisticsSheetRepository>();
        services.AddScoped<ILotTraceabilityRepository, LotTraceabilityRepository>();
        services.AddScoped<ILotHoldRepository, LotHoldRepository>();
        services.AddScoped<IRecallRepository, RecallRepository>();
        services.AddScoped<IProcessRecordRepository, ProcessRecordRepository>();
        services.AddScoped<ILotHoldEnforcementService, LotHoldEnforcementService>();
        services.AddScoped<IESignatureService, ESignatureService>();
        services.AddScoped<ISerialUnitRepository, SerialUnitRepository>();
        services.AddScoped<ISerialNumberGenerationService, SerialNumberGenerationService>();
        services.AddScoped<IMoldCompatibilityRepository, MoldCompatibilityRepository>();
        services.AddScoped<IMoldAssignmentRepository, MoldAssignmentRepository>();
        services.AddScoped<IMaterialBlendLogRepository, MaterialBlendLogRepository>();
        services.AddScoped<IFabricRollRepository, FabricRollRepository>();
        services.AddScoped<ICutOrderRepository, CutOrderRepository>();
        services.AddScoped<IBundleRepository, BundleRepository>();
        services.AddScoped<IPackagingRepository, PackagingRepository>();
        services.AddScoped<IDisassemblyOrderRepository, DisassemblyOrderRepository>();
        services.AddScoped<IProductionPlanByOrderRepository, ProductionPlanByOrderRepository>();
        services.AddScoped<ISamplingMethodRepository, SamplingMethodRepository>();
        services.AddScoped<IMaterialPurchaseRequestRepository, MaterialPurchaseRequestRepository>();
        services.AddScoped<IQualityInspectionVoucherRepository, QualityInspectionVoucherRepository>();
        services.AddScoped<IQualityInspectionRequestRepository, QualityInspectionRequestRepository>();
        services.AddScoped<IQualityCriteriaGroupRepository, QualityCriteriaGroupRepository>();
        services.AddScoped<IQualityCriteriaRepository, QualityCriteriaRepository>();
        services.AddScoped<IQualityStandardSetRepository, QualityStandardSetRepository>();
        services.AddScoped<IProductionProcessRepository, ProductionProcessRepository>();
        services.AddScoped<IProductionProcessStepRepository, ProductionProcessStepRepository>();
        services.AddScoped<IScrapTransactionRepository, ScrapTransactionRepository>();
        services.AddScoped<IReworkOrderRepository, ReworkOrderRepository>();
        services.AddScoped<ILaborGradeRepository, LaborGradeRepository>();
        services.AddScoped<IMachineCostRateRepository, MachineCostRateRepository>();
        services.AddScoped<IEnergyTariffRepository, EnergyTariffCostRepository>();
        services.AddScoped<IMachineEnergyProfileRepository, MachineEnergyProfileRepository>();
        services.AddScoped<IItemCostHistoryRepository, ItemCostHistoryRepository>();
        services.AddScoped<IQualityCostSummaryRepository, QualityCostSummaryRepository>();
        services.AddScoped<IMaintenanceOrderRepository, MaintenanceOrderRepository>();
        services.AddScoped<IMachineTcoRepository, MachineTcoRepository>();
        services.AddScoped<IEnergyRepository, EnergyRepository>();

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
        services.AddScoped<IInspectionResultRepository, InspectionResultRepository>();
        services.AddScoped<INcrRepository, NcrRepository>();
        services.AddScoped<IDefectLifecycleRepository, DefectLifecycleRepository>();
        services.AddScoped<IInlineInspectionRepository, InlineInspectionRepository>();
        services.AddScoped<IAQLInspectionRepository, AQLInspectionRepository>();
        services.AddSingleton<Application.Quality.Services.IAQLSamplingTableService, AQLSamplingTableService>();

        // rules / sop / lab / labels / reminders
        services.AddScoped<ISopRepository, SopRepository>();
        services.AddScoped<ILabRepository, LabRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddHostedService<ReminderEvaluationService>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddSingleton<RuleEvaluationService>();
        services.AddScoped<IEventHandler<MachineSignalIngestedEvent>, SignalThresholdRuleHandler>();

        // iot repositories
        services.AddScoped<IAdapterRepository, AdapterRepository>();
        services.AddScoped<ISignalMappingRepository, SignalMappingRepository>();
        services.AddScoped<IMachineStateRuleRepository, MachineStateRuleRepository>();
        services.AddScoped<ISignalTagRepository, SignalTagRepository>();
        services.AddScoped<IAdapterHealthRepository, AdapterHealthRepository>();

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
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

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

        // IoT Pipeline
        services.Configure<IotPipelineOptions>(configuration.GetSection("IoT:Pipeline"));
        services.AddSingleton<ISignalConfigCache, SignalConfigCache>();
        services.AddSingleton<DeadbandFilter>();
        services.AddSingleton<ISignalIngestionPipeline, SignalIngestionPipeline>();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<IotPipelineOptions>>().Value);
        services.AddHostedService(sp =>
            new PipelineConsumerService(
                sp.GetRequiredService<ISignalIngestionPipeline>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IOptions<IotPipelineOptions>>().Value,
                sp.GetRequiredService<ILogger<PipelineConsumerService>>(),
                sp.GetService<IIotSignalNotifier>()));

        // MQTT adapter manager — starts one MqttAdapterService per enabled MQTT adapter
        services.AddHostedService(sp =>
            new MqttAdapterManager(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ISignalIngestionPipeline>(),
                sp.GetRequiredService<ILogger<MqttAdapterManager>>(),
                sp.GetRequiredService<ILoggerFactory>()));

        // OPC-UA adapter manager — starts one OpcUaAdapterService per enabled OPC-UA adapter
        services.AddHostedService(sp =>
            new OpcUaAdapterManager(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ISignalIngestionPipeline>(),
                sp.GetRequiredService<ILogger<OpcUaAdapterManager>>(),
                sp.GetRequiredService<ILoggerFactory>()));

        // Modbus TCP adapter manager — starts one ModbusTcpAdapterService per enabled Modbus adapter
        services.AddHostedService(sp =>
            new ModbusTcpAdapterManager(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ISignalIngestionPipeline>(),
                sp.GetRequiredService<ILogger<ModbusTcpAdapterManager>>(),
                sp.GetRequiredService<ILoggerFactory>()));

        // Machine State Engine
        services.AddSingleton<SignalValueCache>();
        services.AddSingleton<MachineStateEvaluator>();
        services.AddScoped<MachineSignalStateHandler>(sp => new MachineSignalStateHandler(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<SignalValueCache>(),
            sp.GetRequiredService<MachineStateEvaluator>(),
            sp.GetRequiredService<ILogger<MachineSignalStateHandler>>(),
            sp.GetService<IIotSignalNotifier>()));
        services.AddScoped<IEventHandler<MachineSignalIngestedEvent>>(
            sp => sp.GetRequiredService<MachineSignalStateHandler>());
        services.AddHostedService<StaleSignalWatchdog>();

        // Time-Series Rollup & Retention
        services.AddHostedService<SignalRollupService>();

        // Adapter Health Monitoring
        services.AddHostedService<AdapterWatchdogService>();

        // Storage
        var storageSection = configuration.GetSection("Storage");
        services.Configure<StorageOptions>(storageSection);
        var provider = storageSection["Provider"] ?? "Local";
        if (provider.Equals("S3", StringComparison.OrdinalIgnoreCase))
        {
            var s3Opts = storageSection.GetSection("S3").Get<S3StorageOptions>() ?? new();
            services.AddSingleton<Amazon.S3.IAmazonS3>(_ =>
            {
                var s3Config = new Amazon.S3.AmazonS3Config
                {
                    ServiceURL = s3Opts.ServiceUrl,
                    ForcePathStyle = s3Opts.ForcePathStyle,
                };
                return new Amazon.S3.AmazonS3Client(s3Opts.AccessKey, s3Opts.SecretKey, s3Config);
            });
            services.AddScoped<IFileStorage, S3Storage>();
        }
        else
        {
            services.AddScoped<IFileStorage, LocalDiskStorage>();
        }
        services.AddScoped<IFileObjectRepository, FileObjectRepository>();
        services.AddScoped<IThumbnailGenerator, ImageSharpThumbnailGenerator>();

        // Labeling
        services.AddSingleton<IIdentityEncodingService, IdentityEncodingService>();
        services.AddSingleton<ILabelRenderer, QuestPdfLabelRenderer>();

        return services;
    }
}
