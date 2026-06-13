using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Domain.Common;
using Microsoft.AspNetCore.Identity;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.ValueObjects;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Lab;
using AeroMes.Domain.Rules;
using AeroMes.Domain.Settings;
using AeroMes.Domain.Wms;
using AeroMes.Infrastructure.Identity;
using LiteBus.Events.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IEventMediator eventMediator)
    : IdentityDbContext<ApplicationUser>(options), IUnitOfWork
{

    // auth schema
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    // master schema
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Routing> Routings => Set<Routing>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<ShiftTemplate> ShiftTemplates => Set<ShiftTemplate>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<ShiftBreak> ShiftBreaks => Set<ShiftBreak>();
    public DbSet<WorkCalendar> WorkCalendars => Set<WorkCalendar>();
    public DbSet<CalendarDay> CalendarDays => Set<CalendarDay>();
    public DbSet<CalendarShift> CalendarShifts => Set<CalendarShift>();
    public DbSet<CalendarException> CalendarExceptions => Set<CalendarException>();
    public DbSet<DowntimeReasonCode> DowntimeReasonCodes => Set<DowntimeReasonCode>();
    public DbSet<MachineProductConfig> MachineProductConfigs => Set<MachineProductConfig>();
    public DbSet<AlertThreshold> AlertThresholds => Set<AlertThreshold>();
    public DbSet<WorkOrderAutoRules> WorkOrderAutoRules => Set<WorkOrderAutoRules>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ApprovedVendorItem> ApprovedVendorItems => Set<ApprovedVendorItem>();
    public DbSet<CapabilityGroup> CapabilityGroups => Set<CapabilityGroup>();
    public DbSet<CapabilityGroupMember> CapabilityGroupMembers => Set<CapabilityGroupMember>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPartNumber> CustomerPartNumbers => Set<CustomerPartNumber>();
    public DbSet<CustomerQualitySpec> CustomerQualitySpecs => Set<CustomerQualitySpec>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeAssignment> ProductAttributeAssignments => Set<ProductAttributeAssignment>();
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<ProductionTeam> ProductionTeams => Set<ProductionTeam>();
    public DbSet<Mold> Molds => Set<Mold>();
    public DbSet<MoldProductMapping> MoldProductMappings => Set<MoldProductMapping>();
    public DbSet<MoldMaintenanceLog> MoldMaintenanceLogs => Set<MoldMaintenanceLog>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<BomHeader> BomHeaders => Set<BomHeader>();
    public DbSet<BomByProduct> BomByProducts => Set<BomByProduct>();
    public DbSet<SubstituteMaterial> SubstituteMaterials => Set<SubstituteMaterial>();
    public DbSet<DisassemblyBom> DisassemblyBoms => Set<DisassemblyBom>();
    public DbSet<DisassemblyBomLine> DisassemblyBomLines => Set<DisassemblyBomLine>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<EngChange> EngChanges => Set<EngChange>();
    public DbSet<ToolOperationMapping> ToolOperationMappings => Set<ToolOperationMapping>();
    public DbSet<ToolCheckout> ToolCheckouts => Set<ToolCheckout>();
    public DbSet<ToolMaintenanceLog> ToolMaintenanceLogs => Set<ToolMaintenanceLog>();
    public DbSet<MachineProductParam> MachineProductParams => Set<MachineProductParam>();
    public DbSet<OperatorCertification> OperatorCertifications => Set<OperatorCertification>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    // integration schema
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    // prod schema
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();

    // wms schema
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<Aisle> Aisles => Set<Aisle>();
    public DbSet<Rack> Racks => Set<Rack>();
    public DbSet<Bin> Bins => Set<Bin>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GrnLine> GrnLines => Set<GrnLine>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<BeginningInventoryEntry> BeginningInventoryEntries => Set<BeginningInventoryEntry>();
    public DbSet<FactoryWarehouseReceipt> FactoryWarehouseReceipts => Set<FactoryWarehouseReceipt>();
    public DbSet<FactoryReceiptLine> FactoryReceiptLines => Set<FactoryReceiptLine>();
    public DbSet<FactoryWarehouseExport> FactoryWarehouseExports => Set<FactoryWarehouseExport>();
    public DbSet<FactoryExportLine> FactoryExportLines => Set<FactoryExportLine>();
    public DbSet<MaterialTransferSlip> MaterialTransferSlips => Set<MaterialTransferSlip>();
    public DbSet<MaterialTransferLine> MaterialTransferLines => Set<MaterialTransferLine>();
    public DbSet<MaterialSupplyRequest> MaterialSupplyRequests => Set<MaterialSupplyRequest>();
    public DbSet<MaterialSupplyRequestLine> MaterialSupplyRequestLines => Set<MaterialSupplyRequestLine>();
    public DbSet<MaterialRequisition> MaterialRequisitions => Set<MaterialRequisition>();
    public DbSet<MaterialRequisitionLine> MaterialRequisitionLines => Set<MaterialRequisitionLine>();
    public DbSet<FinishedProductIntakeRequest> FinishedProductIntakeRequests => Set<FinishedProductIntakeRequest>();
    public DbSet<IntakeRequestLine> IntakeRequestLines => Set<IntakeRequestLine>();
    public DbSet<CycleCountPlan> CycleCountPlans => Set<CycleCountPlan>();
    public DbSet<CycleCountLine> CycleCountLines => Set<CycleCountLine>();
    public DbSet<StockPolicy> StockPolicies => Set<StockPolicy>();
    public DbSet<ReplenishmentAlert> ReplenishmentAlerts => Set<ReplenishmentAlert>();
    public DbSet<ReturnMerchandiseAuthorization> Rmas => Set<ReturnMerchandiseAuthorization>();
    public DbSet<RmaLine> RmaLines => Set<RmaLine>();
    public DbSet<ShipmentOrder> ShipmentOrders => Set<ShipmentOrder>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<PickList> PickLists => Set<PickList>();
    public DbSet<PickListLine> PickListLines => Set<PickListLine>();
    public DbSet<Carton> Cartons => Set<Carton>();
    public DbSet<CartonContent> CartonContents => Set<CartonContent>();

    // qual schema
    public DbSet<DefectCode> DefectCodes => Set<DefectCode>();
    public DbSet<DefectDetail> DefectDetails => Set<DefectDetail>();
    public DbSet<InspectionPlan> InspectionPlans => Set<InspectionPlan>();
    public DbSet<InspectionCharacteristic> InspectionCharacteristics => Set<InspectionCharacteristic>();
    public DbSet<InspectionOrder> InspectionOrders => Set<InspectionOrder>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();
    public DbSet<Ncr> Ncrs => Set<Ncr>();
    public DbSet<NcrDefectLine> NcrDefectLines => Set<NcrDefectLine>();

    // iot schema
    public DbSet<AdapterInstance> AdapterInstances => Set<AdapterInstance>();
    public DbSet<SignalMapping> SignalMappings => Set<SignalMapping>();
    public DbSet<MachineStateRule> MachineStateRules => Set<MachineStateRule>();
    public DbSet<SignalTag> SignalTags => Set<SignalTag>();
    public DbSet<MachineSignalLog> MachineSignalLogs => Set<MachineSignalLog>();
    public DbSet<MachineStateSnapshot> MachineStateSnapshots => Set<MachineStateSnapshot>();
    public DbSet<MachineStateHistory> MachineStateHistories => Set<MachineStateHistory>();
    public DbSet<SignalAgg1min> SignalAgg1mins => Set<SignalAgg1min>();
    public DbSet<SignalAgg1hr> SignalAgg1hrs => Set<SignalAgg1hr>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();
    public DbSet<AdapterHealth> AdapterHealths => Set<AdapterHealth>();
    public DbSet<AdapterHealthLog> AdapterHealthLogs => Set<AdapterHealthLog>();

    // rules
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleCondition> RuleConditions => Set<RuleCondition>();
    public DbSet<RuleAction> RuleActions => Set<RuleAction>();
    public DbSet<RuleExecutionLog> RuleExecutionLogs => Set<RuleExecutionLog>();

    // lab
    public DbSet<TestMethod> TestMethods => Set<TestMethod>();
    public DbSet<TestPanel> TestPanels => Set<TestPanel>();
    public DbSet<TestPanelItem> TestPanelItems => Set<TestPanelItem>();
    public DbSet<LabRequest> LabRequests => Set<LabRequest>();
    public DbSet<LabSample> LabSamples => Set<LabSample>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<LabReport> LabReports => Set<LabReport>();

    // settings
    public DbSet<SystemOptions> SystemOptions => Set<SystemOptions>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync(ct);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in events)
            await eventMediator.PublishAsync(domainEvent, null, ct);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        ConfigureAuthSchema(b);
        ConfigureMasterSchema(b);
        ConfigureIntegrationSchema(b);
        ConfigureProdSchema(b);
        ConfigureWmsSchema(b);
        ConfigureQualSchema(b);
        ConfigureSettingsSchema(b);
        ConfigureIotSchema(b);
        ConfigureRulesSchema(b);
        ConfigureLabSchema(b);
    }

    // ── auth ──────────────────────────────────────────────────────────────
    private static void ConfigureAuthSchema(ModelBuilder b)
    {
        b.Entity<Permission>(e =>
        {
            e.ToTable("Permissions", "auth");
            e.HasKey(x => x.PermissionId);
            e.Property(x => x.Resource).HasMaxLength(50).IsRequired();
            e.Property(x => x.Action).HasMaxLength(30).IsRequired();
            e.Property(x => x.PermissionCode).HasMaxLength(82).IsRequired();
            e.Property(x => x.Description).HasMaxLength(200);
            e.HasIndex(x => x.PermissionCode).IsUnique();
            e.HasIndex(x => new { x.Resource, x.Action }).IsUnique();
        });

        b.Entity<RolePermission>(e =>
        {
            e.ToTable("RolePermissions", "auth");
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.Property(x => x.RoleId).HasMaxLength(450);
            e.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Permission>()
                .WithMany()
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<UserPermissionOverride>(e =>
        {
            e.ToTable("UserPermissionOverrides", "auth");
            e.HasKey(x => x.OverrideId);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.GrantedBy).HasMaxLength(450).IsRequired();
            e.Property(x => x.Effect).HasConversion<string>().HasMaxLength(10);
            e.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.PermissionId });
        });

        b.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.EmployeeCode).HasMaxLength(50);
            e.Property(x => x.FullName).HasMaxLength(250);
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.PreferredLanguage).HasMaxLength(10);
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
        });

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens", "auth");
            e.HasKey(x => x.TokenId);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.Property(x => x.DeviceInfo).HasMaxLength(200);
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.Ignore(x => x.IsActive);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.FamilyId);
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SecurityAuditLog>(e =>
        {
            e.ToTable("SecurityAuditLog", "auth");
            e.HasKey(x => x.AuditId);
            e.Property(x => x.AuditId).UseIdentityColumn();
            e.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            e.Property(x => x.ActorId).HasMaxLength(450);
            e.Property(x => x.ActorType).HasMaxLength(20);
            e.Property(x => x.ActorIp).HasMaxLength(45);
            e.Property(x => x.ActorUserAgent).HasMaxLength(500);
            e.Property(x => x.TargetType).HasMaxLength(50);
            e.Property(x => x.TargetId).HasMaxLength(200);
            e.Property(x => x.Outcome).HasMaxLength(10).IsRequired();
            e.Property(x => x.FailureReason).HasMaxLength(500);
            e.HasIndex(x => new { x.ActorId, x.OccurredAt });
            e.HasIndex(x => new { x.EventType, x.OccurredAt });
            e.HasIndex(x => new { x.TargetType, x.TargetId, x.OccurredAt });
            // Append-only: prevent EF from generating UPDATE/DELETE for this entity
            e.ToTable(t => t.ExcludeFromMigrations(false));
        });

        b.Entity<ApiKey>(e =>
        {
            e.ToTable("ApiKeys", "auth");
            e.HasKey(x => x.ApiKeyId);
            e.Property(x => x.KeyName).HasMaxLength(100).IsRequired();
            e.Property(x => x.KeyPrefix).HasMaxLength(8).IsRequired();
            e.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();
            e.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.AssignedRole).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.KeyHash).IsUnique();
            e.HasIndex(x => x.OwnerUserId);
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Passkey (WebAuthn) — IdentityUserPasskey<TKey> is not auto-discovered; must be configured manually
        b.Entity<IdentityUserPasskey<string>>(e =>
        {
            e.ToTable("AspNetUserPasskeys");
            e.HasKey(x => new { x.UserId, x.CredentialId });
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.CredentialId).IsRequired();
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsOne(x => x.Data, d =>
            {
                d.ToJson();
                d.Property(p => p.Name).HasMaxLength(200);
            });
        });
    }

    // ── master ────────────────────────────────────────────────────────────
    private static void ConfigureMasterSchema(ModelBuilder b)
    {
        b.Entity<WorkCenter>(e =>
        {
            e.ToTable("WorkCenters", "master");
            e.HasKey(x => x.WorkCenterID);
            e.Property(x => x.WorkCenterCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.WorkCenterName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.WorkCenterCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<Machine>(e =>
        {
            e.ToTable("Machines", "master");
            e.HasKey(x => x.MachineCode);
            e.Property(x => x.MachineCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.MachineName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MachineCategory).HasMaxLength(30);
            e.Property(x => x.TargetOeePct).HasColumnType("NUMERIC(5,2)");
            e.Property(x => x.TheoreticalCapacityPerHour).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.HourlyCostRate).HasColumnType("DECIMAL(18,2)");
            e.Property(x => x.OpcUaNodeId).HasMaxLength(200);
            e.Property(x => x.CertificationCode).HasMaxLength(30);

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<MachineProductParam>(e =>
        {
            e.ToTable("MachineProductParams", "master");
            e.HasKey(x => x.ParamId);
            e.Property(x => x.ParamId).UseIdentityColumn();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ParamName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(20);
            e.Property(x => x.NominalValue).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.MinValue).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.MaxValue).HasColumnType("NUMERIC(18,4)");
            e.HasIndex(x => new { x.MachineCode, x.ProductCode, x.ParamName }).IsUnique();

            e.HasOne<Machine>()
                .WithMany()
                .HasForeignKey(x => x.MachineCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<OperatorCertification>(e =>
        {
            e.ToTable("OperatorCertifications", "master");
            e.HasKey(x => x.CertId);
            e.Property(x => x.CertId).UseIdentityColumn();
            e.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.CertificationCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.IssuedDate).HasColumnType("date");
            e.Property(x => x.ExpiryDate).HasColumnType("date");
            e.Property(x => x.IssuedBy).HasMaxLength(100);
            e.HasIndex(x => new { x.EmployeeCode, x.CertificationCode, x.IsActive });

            e.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductCategory>(e =>
        {
            e.ToTable("ProductCategories", "master");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.StandardProductionTime).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.Color).HasMaxLength(20);
            e.HasIndex(x => x.CategoryCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x => x.Children)
                .HasField("_children")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<UnitOfMeasure>(e =>
        {
            e.ToTable("UnitsOfMeasure", "master");
            e.HasKey(x => x.UoMCode);
            e.Property(x => x.UoMCode).HasMaxLength(20).ValueGeneratedNever();
            e.Property(x => x.UoMName).HasMaxLength(50).IsRequired();
            e.Property(x => x.UoMGroup).HasMaxLength(20).IsRequired();
        });

        b.Entity<Product>(e =>
        {
            e.ToTable("Products", "master");
            e.HasKey(x => x.ProductCode);
            e.Property(x => x.ProductCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.BarcodePattern).HasMaxLength(200);
            e.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(10).IsRequired();
            e.Property(x => x.BaseUoMCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.PurchaseUoMCode).HasMaxLength(20);
            e.Property(x => x.PurchaseToBaseQty).HasColumnType("NUMERIC(18,6)");
            e.Property(x => x.NetWeight).HasColumnType("NUMERIC(10,4)");
            e.Property(x => x.GrossWeight).HasColumnType("NUMERIC(10,4)");
            e.Property(x => x.Length).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.Width).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.Height).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.ReorderPoint).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.SafetyStock).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ProcurementType).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.LifecycleStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.EffectiveFrom).HasColumnType("date");
            e.Property(x => x.EffectiveTo).HasColumnType("date");
            e.Property(x => x.CustomerPartNo).HasMaxLength(100);
            e.Property(x => x.DrawingNo).HasMaxLength(100);
            e.Property(x => x.Revision).HasMaxLength(10);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            e.Property(x => x.FixedPurchasePrice).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.TechnicalStandard).HasMaxLength(200);
            e.Property(x => x.QuantityFormula).HasMaxLength(500);
            e.Property(x => x.PickingStrategy).HasConversion<string>().HasMaxLength(10);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.UoMConversions)
                .WithOne()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.UoMConversions)
                .HasField("_uomConversions")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.Property(x => x.ParentProductCode).HasMaxLength(50);
            e.HasIndex(x => x.ParentProductCode);
            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ParentProductCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Specifications)
                .WithOne()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Specifications)
                .HasField("_specifications")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<UnitOfMeasure>()
                .WithMany()
                .HasForeignKey(x => x.BaseUoMCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<UnitOfMeasure>()
                .WithMany()
                .HasForeignKey(x => x.PurchaseUoMCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductSpecification>(e =>
        {
            e.ToTable("ProductSpecifications", "master");
            e.HasKey(x => x.SpecificationId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.SpecCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => new { x.ProductCode, x.SpecCode }).IsUnique();
        });

        b.Entity<ProductUoMConversion>(e =>
        {
            e.ToTable("ProductUoMConversions", "master");
            e.HasKey(x => x.ConversionId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UoMCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.ToBaseFactor).HasColumnType("NUMERIC(18,6)");
            e.Property(x => x.Notes).HasMaxLength(255);
            e.HasIndex(x => new { x.ProductCode, x.UoMCode }).IsUnique();

            e.HasOne<UnitOfMeasure>().WithMany()
                .HasForeignKey(x => x.UoMCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductAttribute>(e =>
        {
            e.ToTable("ProductAttributes", "master");
            e.HasKey(x => x.AttributeId);
            e.Property(x => x.AttributeCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.AttributeName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.AttributeCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Values)
                .WithOne()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Values)
                .HasField("_values")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ProductAttributeValue>(e =>
        {
            e.ToTable("ProductAttributeValues", "master");
            e.HasKey(x => x.ValueId);
            e.Property(x => x.Value).HasMaxLength(100).IsRequired();
            e.Property(x => x.GroupName).HasMaxLength(100);
            e.HasIndex(x => new { x.AttributeId, x.Value }).IsUnique();
        });

        b.Entity<ProductAttributeAssignment>(e =>
        {
            e.ToTable("ProductAttributeAssignments", "master");
            e.HasKey(x => x.AssignmentId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.ProductCode, x.AttributeId }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Attribute).WithMany()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SelectedValue).WithMany()
                .HasForeignKey(x => x.SelectedValueId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<BomItem>(e =>
        {
            e.ToTable("BOM", "master");
            e.HasKey(x => x.BomID);
            e.Property(x => x.ParentProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ChildProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.RequiredQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ScrapFactor).HasColumnType("NUMERIC(5,2)");
            e.HasIndex(x => new { x.ParentProductCode, x.ChildProductCode }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ParentProductCode);
            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ChildProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Operation>(e =>
        {
            e.ToTable("Operations", "master");
            e.HasKey(x => x.OperationCode);
            e.Property(x => x.OperationCode).HasMaxLength(30).ValueGeneratedNever();
            e.Property(x => x.OperationName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<CapabilityGroup>(e =>
        {
            e.ToTable("CapabilityGroups", "master");
            e.HasKey(x => x.GroupCode);
            e.Property(x => x.GroupCode).HasMaxLength(30).ValueGeneratedNever();
            e.Property(x => x.GroupName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<CapabilityGroupMember>(e =>
        {
            e.ToTable("CapabilityGroupMembers", "master");
            e.HasKey(x => x.MemberId);
            e.Property(x => x.MemberId).ValueGeneratedOnAdd();
            e.Property(x => x.GroupCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.ResourceId).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.GroupCode, x.ResourceType, x.ResourceId }).IsUnique();
            e.HasOne<CapabilityGroup>().WithMany().HasForeignKey(x => x.GroupCode).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Routing>(e =>
        {
            e.ToTable("Routings", "master");
            e.HasKey(x => x.RoutingID);
            e.Property(x => x.RoutingCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.RoutingName).HasMaxLength(150).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoutingCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ProductCode);

            e.HasMany(r => r.Steps)
                .WithOne(s => s.Routing)
                .HasForeignKey(s => s.RoutingID);
            e.Navigation(r => r.Steps)
                .HasField("_steps")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<RoutingStep>(e =>
        {
            e.ToTable("RoutingSteps", "master");
            e.HasKey(x => x.RoutingStepID);
            e.Property(x => x.OperationCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => new { x.RoutingID, x.StepNumber }).IsUnique();

            // Routing ↔ Steps relationship is configured on the Routing side
            e.HasOne(x => x.Operation)
                .WithMany()
                .HasForeignKey(x => x.OperationCode);
            e.HasOne(x => x.DefaultWorkCenter)
                .WithMany()
                .HasForeignKey(x => x.DefaultWorkCenterID);
        });

        b.Entity<StorageLocation>(e =>
        {
            e.ToTable("StorageLocations", "master");
            e.HasKey(x => x.LocationID);
            e.Property(x => x.LocationCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LocationName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LocationType).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.LocationCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID)
                .IsRequired(false);
        });

        b.Entity<Warehouse>(e =>
        {
            e.ToTable("Warehouses", "master");
            e.HasKey(x => x.WarehouseId);
            e.Property(x => x.WarehouseId).UseIdentityColumn();
            e.Property(x => x.WarehouseCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.WarehouseName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.WarehouseType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.IntegrationSource).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.WarehouseCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<ShiftTemplate>(e =>
        {
            e.ToTable("ShiftTemplates", "master");
            e.HasKey(x => x.ShiftCode);
            e.Property(x => x.ShiftCode).HasMaxLength(20).ValueGeneratedNever();
            e.Property(x => x.ShiftName).HasMaxLength(100).IsRequired();
            e.Property(x => x.ValidDays).HasConversion<int>();
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DowntimeReasonCode>(e =>
        {
            e.ToTable("DowntimeReasonCodes", "master");
            e.HasKey(x => x.ReasonCode);
            e.Property(x => x.ReasonCode).HasMaxLength(30).ValueGeneratedNever();
            e.Property(x => x.ReasonName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<MachineProductConfig>(e =>
        {
            e.ToTable("MachineProductConfigs", "master");
            e.HasKey(x => new { x.MachineCode, x.ProductCode });
            e.Property(x => x.MachineCode).HasMaxLength(50);
            e.Property(x => x.ProductCode).HasMaxLength(50);
            e.Property(x => x.EffectiveFrom).HasColumnType("date");

            e.HasOne<Machine>()
                .WithMany()
                .HasForeignKey(x => x.MachineCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RoutingStep>()
                .WithMany()
                .HasForeignKey(x => x.RoutingStepId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AlertThreshold>(e =>
        {
            e.ToTable("AlertThresholds", "master");
            e.HasKey(x => x.ThresholdId);
            e.Property(x => x.MetricKey).HasMaxLength(50).IsRequired();
            e.Property(x => x.Scope).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ScopeId).HasMaxLength(50);
            e.Property(x => x.WarningLevel).HasColumnType("DECIMAL(10,4)");
            e.Property(x => x.CriticalLevel).HasColumnType("DECIMAL(10,4)");
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<WorkOrderAutoRules>(e =>
        {
            e.ToTable("WorkOrderAutoRules", "master");
            e.HasKey(x => x.RuleId);
            e.HasIndex(x => x.WorkCenterId).IsUnique().HasFilter("[WorkCenterId] IS NOT NULL AND [IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<WorkShift>(e =>
        {
            e.ToTable("WorkShifts", "master");
            e.HasKey(x => x.WorkShiftId);
            e.Property(x => x.ShiftCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.ShiftName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.ShiftCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Breaks)
                .WithOne()
                .HasForeignKey(x => x.WorkShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ShiftBreak>(e =>
        {
            e.ToTable("ShiftBreaks", "master");
            e.HasKey(x => x.ShiftBreakId);
        });

        b.Entity<WorkCalendar>(e =>
        {
            e.ToTable("WorkCalendars", "master");
            e.HasKey(x => x.WorkCalendarId);
            e.Property(x => x.CalendarCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.CalendarName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.CalendarCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Days)
                .WithOne()
                .HasForeignKey(x => x.WorkCalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Exceptions)
                .WithOne()
                .HasForeignKey(x => x.WorkCalendarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CalendarDay>(e =>
        {
            e.ToTable("CalendarDays", "master");
            e.HasKey(x => x.CalendarDayId);
            e.HasIndex(x => new { x.WorkCalendarId, x.DayOfWeek }).IsUnique();
            e.Property(x => x.DayOfWeek).HasConversion<int>();

            e.HasMany(x => x.Shifts)
                .WithOne()
                .HasForeignKey(x => x.CalendarDayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CalendarShift>(e =>
        {
            e.ToTable("CalendarShifts", "master");
            e.HasKey(x => x.CalendarShiftId);

            e.HasOne(x => x.WorkShift)
                .WithMany()
                .HasForeignKey(x => x.WorkShiftId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CalendarException>(e =>
        {
            e.ToTable("CalendarExceptions", "master");
            e.HasKey(x => x.CalendarExceptionId);
            e.Property(x => x.ExceptionType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedBy).HasMaxLength(256);
            e.HasIndex(x => new { x.WorkCalendarId, x.Date }).IsUnique();

            e.HasOne(x => x.WorkShift)
                .WithMany()
                .HasForeignKey(x => x.WorkShiftId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Supplier>(e =>
        {
            e.ToTable("Suppliers", "master");
            e.HasKey(x => x.SupplierCode);
            e.Property(x => x.SupplierCode).HasMaxLength(30).ValueGeneratedNever();
            e.Property(x => x.SupplierName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Country).HasMaxLength(50);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.ContactName).HasMaxLength(150);
            e.Property(x => x.TaxCode).HasMaxLength(20);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.AvlItems)
                .WithOne()
                .HasForeignKey(x => x.SupplierCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.AvlItems)
                .HasField("_avlItems")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ApprovedVendorItem>(e =>
        {
            e.ToTable("ApprovedVendorList", "master");
            e.HasKey(x => x.AvlItemId);
            e.Property(x => x.SupplierCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.UnitPrice).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.CurrencyCode).HasMaxLength(10);
            e.Property(x => x.MinOrderQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.AqlLevel).HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.ApprovedFrom).HasColumnType("date");
            e.Property(x => x.ApprovedTo).HasColumnType("date");
            e.HasIndex(x => new { x.SupplierCode, x.ProductCode }).IsUnique();

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Customer>(e =>
        {
            e.ToTable("Customers", "master");
            e.HasKey(x => x.CustomerCode);
            e.Property(x => x.CustomerCode).HasMaxLength(30).ValueGeneratedNever();
            e.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CustomerType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TaxId).HasMaxLength(50);
            e.Property(x => x.Country).HasMaxLength(80);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.ShippingAddress).HasMaxLength(300);
            e.Property(x => x.ContactName).HasMaxLength(150);
            e.Property(x => x.ContactPhone).HasMaxLength(30);
            e.Property(x => x.ContactEmail).HasMaxLength(150);
            e.Property(x => x.Currency).HasMaxLength(3);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.PartNumbers)
                .WithOne()
                .HasForeignKey(x => x.CustomerCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.PartNumbers)
                .HasField("_partNumbers")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.QualitySpecs)
                .WithOne()
                .HasForeignKey(x => x.CustomerCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.QualitySpecs)
                .HasField("_qualitySpecs")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<CustomerPartNumber>(e =>
        {
            e.ToTable("CustomerPartNumbers", "master");
            e.HasKey(x => x.CustomerPartNumberId);
            e.Property(x => x.CustomerCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.CustomerPartNo).HasMaxLength(100).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.DrawingReference).HasMaxLength(100);
            e.Property(x => x.Revision).HasMaxLength(20);
            e.HasIndex(x => new { x.CustomerCode, x.CustomerPartNo }).IsUnique();

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CustomerQualitySpec>(e =>
        {
            e.ToTable("CustomerQualitySpecs", "master");
            e.HasKey(x => x.CustomerQualitySpecId);
            e.Property(x => x.CustomerCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.AqlLevel).HasMaxLength(10);
            e.Property(x => x.InspectionLevel).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.AcceptanceCriteria).HasMaxLength(500);
            e.Property(x => x.SpecialRequirements).HasMaxLength(500);
            e.Property(x => x.EffectiveFrom).HasColumnType("date");
            e.Property(x => x.EffectiveTo).HasColumnType("date");
            e.HasIndex(x => new { x.CustomerCode, x.ProductCode }).IsUnique();

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Employee>(e =>
        {
            e.ToTable("Employees", "master");
            e.HasKey(x => x.EmployeeCode);
            // Length 50 matches prod.Jobs.OperatorID so the FK types line up.
            e.Property(x => x.EmployeeCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.RoleType).HasConversion<string>().HasMaxLength(20);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.DefaultWorkCenter)
                .WithMany()
                .HasForeignKey(x => x.DefaultWorkCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Skills)
                .WithOne()
                .HasForeignKey(x => x.EmployeeCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Skills)
                .HasField("_skills")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.ShiftAssignments)
                .WithOne()
                .HasForeignKey(x => x.EmployeeCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.ShiftAssignments)
                .HasField("_shiftAssignments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<EmployeeSkill>(e =>
        {
            e.ToTable("EmployeeSkills", "master");
            e.HasKey(x => x.EmployeeSkillId);
            e.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.OperationCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.CertifiedAt).HasColumnType("date");
            e.Property(x => x.ExpiresAt).HasColumnType("date");
            e.HasIndex(x => new { x.EmployeeCode, x.OperationCode }).IsUnique();

            e.HasOne(x => x.Operation)
                .WithMany()
                .HasForeignKey(x => x.OperationCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<OrgUnit>(e =>
        {
            e.ToTable("OrgUnits", "master");
            e.HasKey(x => x.UnitId);
            e.Property(x => x.UnitCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitName).HasMaxLength(200).IsRequired();
            e.Property(x => x.UnitType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SourceSystemId).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.UnitCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => x.ParentUnitId);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentUnitId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x => x.Children)
                .HasField("_children")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ProductionTeam>(e =>
        {
            e.ToTable("ProductionTeams", "master");
            e.HasKey(x => x.TeamCode);
            e.Property(x => x.TeamCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.TeamName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductionRate).HasColumnType("NUMERIC(10,2)");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.OrgUnit)
                .WithMany()
                .HasForeignKey(x => x.OrgUnitId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Members)
                .WithOne()
                .HasForeignKey(x => x.TeamCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Members)
                .HasField("_members")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.ProductGroups)
                .WithOne()
                .HasForeignKey(x => x.TeamCode)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.ProductGroups)
                .HasField("_productGroups")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ProductionTeamMember>(e =>
        {
            e.ToTable("ProductionTeamMembers", "master");
            e.HasKey(x => x.MemberId);
            e.Property(x => x.TeamCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.TeamCode, x.EmployeeCode }).IsUnique();

            e.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductionTeamProductGroup>(e =>
        {
            e.ToTable("ProductionTeamProductGroups", "master");
            e.HasKey(x => x.LinkId);
            e.Property(x => x.TeamCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.TeamCode, x.CategoryId }).IsUnique();

            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Mold>(e =>
        {
            e.ToTable("Molds", "master");
            e.HasKey(x => x.MoldId);
            e.Property(x => x.MoldCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.MoldName).HasMaxLength(150).IsRequired();
            e.Property(x => x.MoldType).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.Material).HasMaxLength(100);
            e.Property(x => x.Manufacturer).HasMaxLength(150);
            e.Property(x => x.PurchaseDate).HasColumnType("date");
            e.Property(x => x.PurchaseCost).HasColumnType("DECIMAL(18,2)");
            e.Property(x => x.WeightKg).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.CurrentMachineCode).HasMaxLength(50);
            e.Property(x => x.StorageLocation).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.MoldCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => x.CurrentMachineCode);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.CurrentMachine)
                .WithMany()
                .HasForeignKey(x => x.CurrentMachineCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.ProductMappings)
                .WithOne()
                .HasForeignKey(x => x.MoldId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.ProductMappings)
                .HasField("_productMappings")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.MaintenanceLogs)
                .WithOne()
                .HasForeignKey(x => x.MoldId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.MaintenanceLogs)
                .HasField("_maintenanceLogs")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<MoldProductMapping>(e =>
        {
            e.ToTable("MoldProductMappings", "master");
            e.HasKey(x => x.MappingId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.MoldId, x.ProductCode }).IsUnique();

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MoldMaintenanceLog>(e =>
        {
            e.ToTable("MoldMaintenanceLogs", "master");
            e.HasKey(x => x.LogId);
            e.Property(x => x.MaintenanceType).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.TechnicianId).HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.PartReplaced).HasMaxLength(300);
            e.Property(x => x.Cost).HasColumnType("DECIMAL(18,2)");
            e.HasIndex(x => x.MoldId);
        });

        b.Entity<Tool>(e =>
        {
            e.ToTable("Tools", "master");
            e.HasKey(x => x.ToolId);
            e.Property(x => x.ToolCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ToolName).HasMaxLength(150).IsRequired();
            e.Property(x => x.ToolType).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.Specification).HasMaxLength(300);
            e.Property(x => x.NextCalibrationDue).HasColumnType("date");
            e.Property(x => x.StorageLocation).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.PurchaseDate).HasColumnType("date");
            e.Property(x => x.PurchaseCost).HasColumnType("DECIMAL(18,2)");
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.ToolCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => x.CurrentWorkCenterId);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.CurrentWorkCenter)
                .WithMany()
                .HasForeignKey(x => x.CurrentWorkCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.OperationMappings)
                .WithOne()
                .HasForeignKey(x => x.ToolId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.OperationMappings)
                .HasField("_operationMappings")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.Checkouts)
                .WithOne()
                .HasForeignKey(x => x.ToolId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Checkouts)
                .HasField("_checkouts")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.MaintenanceLogs)
                .WithOne()
                .HasForeignKey(x => x.ToolId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.MaintenanceLogs)
                .HasField("_maintenanceLogs")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ToolOperationMapping>(e =>
        {
            e.ToTable("ToolOperationMappings", "master");
            e.HasKey(x => x.MappingId);
            e.Property(x => x.OperationCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50);
            e.Property(x => x.UsageCountPerOp).HasColumnType("NUMERIC(10,4)");
            e.HasIndex(x => new { x.ToolId, x.OperationCode, x.ProductCode }).IsUnique();

            e.HasOne(x => x.Operation)
                .WithMany()
                .HasForeignKey(x => x.OperationCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ToolCheckout>(e =>
        {
            e.ToTable("ToolCheckouts", "master");
            e.HasKey(x => x.CheckoutId);
            e.Property(x => x.CheckedOutBy).HasMaxLength(100).IsRequired();
            e.Property(x => x.ReturnedBy).HasMaxLength(100);
            e.Property(x => x.ConditionOnReturn).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(255);
            e.HasIndex(x => new { x.ToolId, x.ReturnedAt });

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ToolMaintenanceLog>(e =>
        {
            e.ToTable("ToolMaintenanceLogs", "master");
            e.HasKey(x => x.LogId);
            e.Property(x => x.MaintenanceType).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.PerformedBy).HasMaxLength(100);
            e.Property(x => x.Cost).HasColumnType("DECIMAL(18,2)");
            e.Property(x => x.NextDueDate).HasColumnType("date");
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasIndex(x => x.ToolId);
        });

        b.Entity<BomHeader>(e =>
        {
            e.ToTable("BomHeaders", "master");
            e.HasKey(x => x.BomHeaderId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Version).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.EffectiveFrom).HasColumnType("date");
            e.Property(x => x.EffectiveTo).HasColumnType("date");
            e.Property(x => x.BaseQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.EcoReference).HasMaxLength(50);
            e.Property(x => x.ApprovedBy).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => new { x.ProductCode, x.Version }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.ProductCode, x.Status });
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.BomType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.IsDefault).HasDefaultValue(false);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.BomHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(x => x.ByProducts).WithOne().HasForeignKey(x => x.BomHeaderId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_byProducts");
            e.Navigation(x => x.ByProducts).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<BomLine>(e =>
        {
            e.ToTable("BomLines", "master");
            e.HasKey(x => x.BomLineId);
            e.Property(x => x.ComponentCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.RequiredQty).HasColumnType("NUMERIC(18,6)");
            e.Property(x => x.UoMCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.ScrapFactor).HasColumnType("NUMERIC(5,2)");
            e.Property(x => x.Notes).HasMaxLength(200);
            e.HasIndex(x => new { x.BomHeaderId, x.LineNo }).IsUnique();

            e.HasOne(x => x.Component)
                .WithMany()
                .HasForeignKey(x => x.ComponentCode)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<UnitOfMeasure>()
                .WithMany()
                .HasForeignKey(x => x.UoMCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<BomByProduct>(b =>
        {
            b.ToTable("BomByProducts", "master");
            b.HasKey(x => x.BomByProductId);
            b.Property(x => x.BomByProductId).UseIdentityColumn();
            b.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            b.HasOne(x => x.ByProduct).WithMany().HasForeignKey(x => x.ByProductCode)
                .HasPrincipalKey(p => p.ProductCode).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<SubstituteMaterial>(b =>
        {
            b.ToTable("SubstituteMaterials", "master");
            b.HasKey(x => x.SubstituteId);
            b.Property(x => x.SubstituteId).UseIdentityColumn();
            b.HasIndex(x => x.SubstituteCode).IsUnique();
            b.Property(x => x.ConversionRatio).HasColumnType("NUMERIC(18,6)");
            b.HasQueryFilter(x => !x.IsDeleted);
            b.HasOne(x => x.PrimaryMaterial).WithMany().HasForeignKey(x => x.PrimaryMaterialCode)
                .HasPrincipalKey(p => p.ProductCode).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.SubstituteMaterialProduct).WithMany().HasForeignKey(x => x.SubstituteMaterialCode)
                .HasPrincipalKey(p => p.ProductCode).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DisassemblyBom>(b =>
        {
            b.ToTable("DisassemblyBoms", "master");
            b.HasKey(x => x.DisassemblyBomId);
            b.Property(x => x.DisassemblyBomId).UseIdentityColumn();
            b.HasIndex(x => x.BomCode).IsUnique();
            b.Property(x => x.LossRatio).HasColumnType("NUMERIC(18,4)");
            b.HasQueryFilter(x => !x.IsDeleted);
            b.HasOne(x => x.SourceProduct).WithMany().HasForeignKey(x => x.SourceProductCode)
                .HasPrincipalKey(p => p.ProductCode).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.DisassemblyBomId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<DisassemblyBomLine>(b =>
        {
            b.ToTable("DisassemblyBomLines", "master");
            b.HasKey(x => x.LineId);
            b.Property(x => x.LineId).UseIdentityColumn();
            b.Property(x => x.RecoveryRate).HasColumnType("NUMERIC(18,4)");
            b.Property(x => x.FixedQuantity).HasColumnType("NUMERIC(18,4)");
            b.HasOne(x => x.Component).WithMany().HasForeignKey(x => x.ComponentCode)
                .HasPrincipalKey(p => p.ProductCode).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EngChange>(e =>
        {
            e.ToTable("EngChanges", "master");
            e.HasKey(x => x.EcId);
            e.Property(x => x.EcNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.EcType).HasConversion<string>().HasMaxLength(10).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Reason).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(10).IsRequired();
            e.Property(x => x.RequestedBy).HasMaxLength(100);
            e.Property(x => x.TargetDate).HasColumnType("date");
            e.Property(x => x.ApprovedBy).HasMaxLength(100);
            e.Property(x => x.AffectedProducts).HasMaxLength(500);
            e.Property(x => x.SourceEcrNumber).HasMaxLength(30);
            e.HasIndex(x => x.EcNumber).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<BomHeader>()
                .WithMany()
                .HasForeignKey(x => x.NewBomHeaderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ShiftAssignment>(e =>
        {
            e.ToTable("ShiftAssignments", "master");
            e.HasKey(x => x.ShiftAssignmentId);
            e.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ShiftCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.ValidFrom).HasColumnType("date");
            e.Property(x => x.ValidTo).HasColumnType("date");
            e.HasIndex(x => new { x.EmployeeCode, x.ValidFrom });

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ShiftTemplate)
                .WithMany()
                .HasForeignKey(x => x.ShiftCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── integration ───────────────────────────────────────────────────────
    private static void ConfigureIntegrationSchema(ModelBuilder b)
    {
        b.Entity<SalesOrder>(e =>
        {
            e.ToTable("SalesOrders", "integration");
            e.HasKey(x => x.SOID);
            e.Property(x => x.SOCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.CustomerName).HasMaxLength(150);
            e.Property(x => x.CustomerCode).HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.SOCode).IsUnique();

            e.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductionOrder>(e =>
        {
            e.ToTable("ProductionOrders", "integration");
            e.HasKey(x => x.POID);
            e.Property(x => x.POCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.POCode).IsUnique();

            e.HasOne(x => x.SalesOrder)
                .WithMany()
                .HasForeignKey(x => x.SOID)
                .IsRequired(false);

            e.HasOne<Product>().WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── prod ──────────────────────────────────────────────────────────────
    private static void ConfigureProdSchema(ModelBuilder b)
    {
        b.Entity<WorkOrder>(e =>
        {
            e.ToTable("WorkOrders", "prod");
            e.HasKey(x => x.WOID);
            e.Property(x => x.WOCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.WOCode).IsUnique();

            e.Property(x => x.TargetQuantity)
                .HasConversion(q => q.Value, v => Quantity.From(v));
            e.Property(x => x.ActualQtyOK)
                .HasConversion(q => q.Value, v => Quantity.From(v));
            e.Property(x => x.ActualQtyNG)
                .HasConversion(q => q.Value, v => Quantity.From(v));

            e.HasOne(x => x.ProductionOrder)
                .WithMany()
                .HasForeignKey(x => x.POID)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.RoutingStep)
                .WithMany()
                .HasForeignKey(x => x.RoutingStepID)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<Job>(e =>
        {
            e.ToTable("Jobs", "prod");
            e.HasKey(x => x.JobID);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ShiftCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.OperatorID).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.WOID);
            e.HasIndex(x => x.MachineCode);

            e.HasOne(x => x.WorkOrder)
                .WithMany()
                .HasForeignKey(x => x.WOID);

            e.HasOne(x => x.Machine)
                .WithMany()
                .HasForeignKey(x => x.MachineCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<ShiftTemplate>()
                .WithMany()
                .HasForeignKey(x => x.ShiftCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Operator)
                .WithMany()
                .HasForeignKey(x => x.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductionLog>(e =>
        {
            e.ToTable("ProductionLogs", "prod");
            e.HasKey(x => x.LogID);
            e.Property(x => x.DeviceIP).HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(255);
            e.Property(x => x.IdempotencyKey).HasMaxLength(36);
            e.HasIndex(x => x.IdempotencyKey).IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
            e.HasIndex(x => x.JobID);
            e.HasIndex(x => x.Timestamp);

            e.HasMany(p => p.DefectDetails)
                .WithOne()
                .HasForeignKey(d => d.LogID);
            e.Navigation(p => p.DefectDetails)
                .HasField("_defectDetails")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasOne(x => x.Job)
                .WithMany()
                .HasForeignKey(x => x.JobID);
        });

        b.Entity<DowntimeLog>(e =>
        {
            e.ToTable("DowntimeLogs", "prod");
            e.HasKey(x => x.DowntimeLogID);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReasonCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ReasonName).HasMaxLength(150);
            e.Property(x => x.OperatorID).HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(255);
            e.Ignore(x => x.DurationMinutes);
            e.HasIndex(x => x.MachineCode);
            e.HasIndex(x => x.StartTime);

            e.HasOne<Machine>()
                .WithMany()
                .HasForeignKey(x => x.MachineCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<DowntimeReasonCode>()
                .WithMany()
                .HasForeignKey(x => x.ReasonCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<InventoryStock>(e =>
        {
            e.ToTable("InventoryStock", "prod");
            e.HasKey(x => x.StockID);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ExpiryDate).HasColumnType("date");
            e.Property(x => x.ManufacturedDate).HasColumnType("date");
            e.HasIndex(x => new { x.LocationID, x.ProductCode, x.LotNumber }).IsUnique();
            e.HasIndex(x => new { x.ProductCode, x.LotNumber });
            e.HasIndex(x => x.BinId).HasFilter("[BinId] IS NOT NULL");

            e.HasOne(x => x.StorageLocation)
                .WithMany()
                .HasForeignKey(x => x.LocationID);

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Bin>()
                .WithMany()
                .HasForeignKey(x => x.BinId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // ── qual ──────────────────────────────────────────────────────────────
    private static void ConfigureQualSchema(ModelBuilder b)
    {
        b.Entity<DefectCode>(e =>
        {
            e.ToTable("DefectCodes", "qual");
            e.HasKey(x => x.DefectCodeID);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.DefectName).HasMaxLength(150).IsRequired();
            e.Property(x => x.DefectCategory).HasMaxLength(100);
            e.HasIndex(x => x.Code).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<DefectDetail>(e =>
        {
            e.ToTable("DefectDetails", "qual");
            e.HasKey(x => x.DefectDetailID);
            e.HasIndex(x => x.LogID);

            e.HasOne(x => x.DefectCode)
                .WithMany()
                .HasForeignKey(x => x.DefectCodeID);

            // ProductionLog ↔ DefectDetails relationship configured on ProductionLog side
        });

        b.Entity<InspectionPlan>(e =>
        {
            e.ToTable("InspectionPlans", "qual");
            e.HasKey(x => x.PlanId);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.SamplingMethod).HasMaxLength(20).IsRequired();
            e.Property(x => x.InspectionType).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.HasMany(x => x.Characteristics)
                .WithOne(c => c.Plan)
                .HasForeignKey(c => c.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Characteristics)
                .HasField("_characteristics")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<InspectionCharacteristic>(e =>
        {
            e.ToTable("InspectionCharacteristics", "qual");
            e.HasKey(x => x.CharId);
            e.Property(x => x.CharName).HasMaxLength(200).IsRequired();
            e.Property(x => x.MeasurementType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(30);
            e.Property(x => x.AttributeSpec).HasMaxLength(200);
            e.Property(x => x.SeverityLevel).HasMaxLength(20).IsRequired();
            e.Property(x => x.DefectCodeLink).HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(255);
        });

        b.Entity<InspectionOrder>(e =>
        {
            e.ToTable("InspectionOrders", "qual");
            e.HasKey(x => x.InspectionOrderId);
            e.HasIndex(x => x.OrderNo).IsUnique();
            e.Property(x => x.OrderNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.TriggeredBy).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(100);
            e.Property(x => x.InspectorCode).HasMaxLength(100);
            e.Property(x => x.WaivedBy).HasMaxLength(100);
            e.Property(x => x.WaivedReason).HasMaxLength(500);
            e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<InspectionResult>(e =>
        {
            e.ToTable("InspectionResults", "qual");
            e.HasKey(x => x.ResultId);
            e.Property(x => x.ResultId).UseIdentityColumn();
            e.Property(x => x.MeasuredValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.AttributeResult).HasMaxLength(10);
            e.Property(x => x.RecordedBy).HasMaxLength(100).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(255);
            e.HasOne(x => x.Characteristic)
                .WithMany()
                .HasForeignKey(x => x.CharId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.InspectionOrderId, x.CharId });
        });

        b.Entity<Ncr>(e =>
        {
            e.ToTable("Ncrs", "qual");
            e.HasKey(x => x.NcrId);
            e.HasIndex(x => x.NcrNo).IsUnique();
            e.Property(x => x.NcrNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(100);
            e.Property(x => x.QtyAffected).HasColumnType("decimal(18,4)");
            e.Property(x => x.DispositionCode).HasMaxLength(30);
            e.Property(x => x.DispositionSetBy).HasMaxLength(100);
            e.Property(x => x.RootCause).HasMaxLength(500);
            e.Property(x => x.CorrectiveAction).HasMaxLength(500);
            e.Property(x => x.PreventiveAction).HasMaxLength(500);
            e.Property(x => x.AssignedTo).HasMaxLength(100);
            e.Property(x => x.ClosedBy).HasMaxLength(100);
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.HasMany(x => x.DefectLines)
                .WithOne(l => l.Ncr)
                .HasForeignKey(l => l.NcrId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.DefectLines)
                .HasField("_defectLines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<NcrDefectLine>(e =>
        {
            e.ToTable("NcrDefectLines", "qual");
            e.HasKey(x => x.LineId);
            e.Property(x => x.Notes).HasMaxLength(255);
            e.HasOne(x => x.DefectCode)
                .WithMany()
                .HasForeignKey(x => x.DefectCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── wms ───────────────────────────────────────────────────────────────
    private static void ConfigureWmsSchema(ModelBuilder b)
    {
        b.Entity<WarehouseZone>(e =>
        {
            e.ToTable("WarehouseZones", "wms");
            e.HasKey(x => x.ZoneId);
            e.Property(x => x.ZoneId).UseIdentityColumn();
            e.Property(x => x.ZoneCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.ZoneName).HasMaxLength(100).IsRequired();
            e.Property(x => x.ZoneType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TemperatureZone).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.ZoneCode).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<Aisle>(e =>
        {
            e.ToTable("Aisles", "wms");
            e.HasKey(x => x.AisleId);
            e.Property(x => x.AisleId).UseIdentityColumn();
            e.Property(x => x.AisleCode).HasMaxLength(20).IsRequired();
            e.HasIndex(x => new { x.ZoneId, x.AisleCode }).IsUnique();

            e.HasOne(x => x.Zone)
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Rack>(e =>
        {
            e.ToTable("Racks", "wms");
            e.HasKey(x => x.RackId);
            e.Property(x => x.RackId).UseIdentityColumn();
            e.Property(x => x.RackCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.MaxWeightKg).HasColumnType("NUMERIC(10,2)");
            e.HasIndex(x => new { x.AisleId, x.RackCode }).IsUnique();

            e.HasOne(x => x.Aisle)
                .WithMany()
                .HasForeignKey(x => x.AisleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Bin>(e =>
        {
            e.ToTable("Bins", "wms");
            e.HasKey(x => x.BinId);
            e.Property(x => x.BinId).UseIdentityColumn();
            e.Property(x => x.BinCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.BinLevel).HasMaxLength(10).IsRequired();
            e.Property(x => x.BinType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MaxQty).HasColumnType("NUMERIC(18,4)");
            e.HasIndex(x => new { x.RackId, x.BinCode }).IsUnique();

            e.HasOne(x => x.Rack)
                .WithMany()
                .HasForeignKey(x => x.RackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PurchaseOrder>(e =>
        {
            e.ToTable("PurchaseOrders", "wms");
            e.HasKey(x => x.PoId);
            e.Property(x => x.PoId).UseIdentityColumn();
            e.Property(x => x.PoCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.SupplierCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.ExpectedDeliveryDate).HasColumnType("date");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.PoCode).IsUnique();

            e.HasOne<Supplier>()
                .WithMany()
                .HasForeignKey(x => x.SupplierCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.PoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<PurchaseOrderLine>(e =>
        {
            e.ToTable("PurchaseOrderLines", "wms");
            e.HasKey(x => x.PoLineId);
            e.Property(x => x.PoLineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.OrderedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ReceivedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.UnitPrice).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ExpectedLotNumber).HasMaxLength(50);

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<GoodsReceiptNote>(e =>
        {
            e.ToTable("GoodsReceiptNotes", "wms");
            e.HasKey(x => x.GrnId);
            e.Property(x => x.GrnId).UseIdentityColumn();
            e.Property(x => x.GrnCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReceivedBy).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.DeliveryNoteRef).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.GrnCode).IsUnique();

            e.HasOne<PurchaseOrder>()
                .WithMany()
                .HasForeignKey(x => x.PoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<StorageLocation>()
                .WithMany()
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.GrnId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<GrnLine>(e =>
        {
            e.ToTable("GrnLines", "wms");
            e.HasKey(x => x.GrnLineId);
            e.Property(x => x.GrnLineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReceivedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.GrossWeightKg).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.QcStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ManufacturedDate).HasColumnType("date");
            e.Property(x => x.ExpiryDate).HasColumnType("date");

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<PurchaseOrderLine>()
                .WithMany()
                .HasForeignKey(x => x.PoLineId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Bin>()
                .WithMany()
                .HasForeignKey(x => x.DestinationBinId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<StockMovement>(e =>
        {
            e.ToTable("StockMovements", "wms");
            e.HasKey(x => x.MovementId);
            e.Property(x => x.MovementId).UseIdentityColumn();
            e.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.Reference).HasMaxLength(100).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.HasIndex(x => new { x.ProductCode, x.LotNumber });
            e.HasIndex(x => x.Reference);

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<StorageLocation>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Bin>()
                .WithMany()
                .HasForeignKey(x => x.BinId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<BeginningInventoryEntry>(e =>
        {
            e.ToTable("BeginningInventoryEntries", "wms");
            e.HasKey(x => x.EntryId);
            e.Property(x => x.EntryId).UseIdentityColumn();
            e.Property(x => x.Period).HasColumnType("date");
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.BeginningQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.LotNumber).HasMaxLength(100);
            e.Property(x => x.ExpirationDate).HasColumnType("date");
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.HasIndex(x => new { x.Period, x.WarehouseId, x.ProductCode, x.LotNumber });
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<FactoryWarehouseReceipt>(e =>
        {
            e.ToTable("FactoryWarehouseReceipts", "wms");
            e.HasKey(x => x.ReceiptId);
            e.Property(x => x.ReceiptId).UseIdentityColumn();
            e.Property(x => x.VoucherNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.ReceiptType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SupplierOrTransferringUnit).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.HasIndex(x => x.VoucherNumber).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<FactoryReceiptLine>(e =>
        {
            e.ToTable("FactoryReceiptLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.SpecificationCode).HasMaxLength(50);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<FactoryWarehouseExport>(e =>
        {
            e.ToTable("FactoryWarehouseExports", "wms");
            e.HasKey(x => x.ExportId);
            e.Property(x => x.ExportId).UseIdentityColumn();
            e.Property(x => x.VoucherNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.ExportType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ReceiverOrReceivingUnit).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.HasIndex(x => x.VoucherNumber).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.ExportId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<FactoryExportLine>(e =>
        {
            e.ToTable("FactoryExportLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.SpecificationCode).HasMaxLength(50);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MaterialTransferSlip>(e =>
        {
            e.ToTable("MaterialTransferSlips", "wms");
            e.HasKey(x => x.SlipId);
            e.Property(x => x.SlipId).UseIdentityColumn();
            e.Property(x => x.VoucherNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.TransferType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.HasIndex(x => x.VoucherNumber).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.SlipId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<MaterialTransferLine>(e =>
        {
            e.ToTable("MaterialTransferLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.SpecificationCode).HasMaxLength(50);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MaterialSupplyRequest>(e =>
        {
            e.ToTable("MaterialSupplyRequests", "wms");
            e.HasKey(x => x.RequestId);
            e.Property(x => x.RequestId).UseIdentityColumn();
            e.Property(x => x.VoucherNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.VoucherNumber).IsUnique();
            e.Property(x => x.RequestType).HasMaxLength(30).HasConversion<string>();
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.RequesterUnit).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<MaterialSupplyRequestLine>(e =>
        {
            e.ToTable("MaterialSupplyRequestLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.RequestedQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.Notes).HasMaxLength(200);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MaterialRequisition>(e =>
        {
            e.ToTable("MaterialRequisitions", "wms");
            e.HasKey(x => x.RequisitionId);
            e.Property(x => x.RequisitionId).UseIdentityColumn();
            e.Property(x => x.RequisitionNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RequisitionNumber).IsUnique();
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.RequesterUnit).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Domain.Integration.ProductionOrder>()
                .WithMany()
                .HasForeignKey(x => x.ProductionOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.RequisitionId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<MaterialRequisitionLine>(e =>
        {
            e.ToTable("MaterialRequisitionLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.RequestedQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ActualIssuedQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.Notes).HasMaxLength(200);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<FinishedProductIntakeRequest>(e =>
        {
            e.ToTable("FinishedProductIntakeRequests", "wms");
            e.HasKey(x => x.IntakeRequestId);
            e.Property(x => x.IntakeRequestId).UseIdentityColumn();
            e.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RequestNumber).IsUnique();
            e.Property(x => x.IntakePurpose).HasMaxLength(40).HasConversion<string>();
            e.Property(x => x.WarehouseType).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.RequesterUnit).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Domain.Integration.ProductionOrder>()
                .WithMany()
                .HasForeignKey(x => x.ProductionOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.IntakeRequestId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<IntakeRequestLine>(e =>
        {
            e.ToTable("IntakeRequestLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(x => x.RequestedQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ActualReceivedQuantity).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.DefectReason).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(200);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CycleCountPlan>(e =>
        {
            e.ToTable("CycleCountPlans", "wms");
            e.HasKey(x => x.PlanId);
            e.Property(x => x.PlanId).UseIdentityColumn();
            e.Property(x => x.PlanCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.PlanCode).IsUnique();
            e.Property(x => x.PlanType).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<CycleCountLine>(e =>
        {
            e.ToTable("CycleCountLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(100).IsRequired();
            e.Property(x => x.BookQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.CountedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.CountedBy).HasMaxLength(450);
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Ignore(x => x.VarianceQty);
            e.Ignore(x => x.VariancePct);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Bin>()
                .WithMany()
                .HasForeignKey(x => x.BinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<StockPolicy>(e =>
        {
            e.ToTable("StockPolicies", "wms");
            e.HasKey(x => x.PolicyId);
            e.Property(x => x.PolicyId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.MinQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.MaxQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.SafetyStockQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ReorderQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasIndex(x => new { x.ProductCode, x.LocationId }).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne<Domain.Master.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Master.StorageLocation>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ReplenishmentAlert>(e =>
        {
            e.ToTable("ReplenishmentAlerts", "wms");
            e.HasKey(x => x.AlertId);
            e.Property(x => x.AlertId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.CurrentQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.AcknowledgedBy).HasMaxLength(450);

            e.HasOne<StockPolicy>()
                .WithMany()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Domain.Wms.PurchaseOrder>()
                .WithMany()
                .HasForeignKey(x => x.LinkedPoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ReturnMerchandiseAuthorization>(e =>
        {
            e.ToTable("ReturnMerchandiseAuthorizations", "wms");
            e.HasKey(x => x.RmaId);
            e.Property(x => x.RmaId).UseIdentityColumn();
            e.Property(x => x.RmaCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RmaCode).IsUnique();
            e.Property(x => x.ReturnDirection).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.Status).HasMaxLength(20).HasConversion<string>();
            e.Property(x => x.SourceDocumentType).HasMaxLength(20);
            e.Property(x => x.ReturnReason).HasMaxLength(500);
            e.Property(x => x.AuthorizedBy).HasMaxLength(450);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(l => l.RmaId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<RmaLine>(e =>
        {
            e.ToTable("RmaLines", "wms");
            e.HasKey(x => x.RmaLineId);
            e.Property(x => x.RmaLineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReturnQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ReceivedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.Disposition).HasMaxLength(30).HasConversion<string>();

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .HasPrincipalKey(p => p.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ShipmentOrder>(e =>
        {
            e.ToTable("ShipmentOrders", "wms");
            e.HasKey(x => x.ShipmentId);
            e.Property(x => x.ShipmentId).UseIdentityColumn();
            e.Property(x => x.ShipmentCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.ShipmentCode).IsUnique();
            e.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
            e.Property(x => x.CarrierName).HasMaxLength(100);
            e.Property(x => x.TrackingNumber).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(l => l.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<ShipmentLine>(e =>
        {
            e.ToTable("ShipmentLines", "wms");
            e.HasKey(x => x.LineId);
            e.Property(x => x.LineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.OrderedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.PickedQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.PackedQty).HasColumnType("NUMERIC(18,4)");

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .HasPrincipalKey(p => p.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PickList>(e =>
        {
            e.ToTable("PickLists", "wms");
            e.HasKey(x => x.PickListId);
            e.Property(x => x.PickListId).UseIdentityColumn();
            e.Property(x => x.AssignedTo).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);

            e.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(l => l.PickListId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_lines");
            e.Navigation(x => x.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<PickListLine>(e =>
        {
            e.ToTable("PickListLines", "wms");
            e.HasKey(x => x.PickLineId);
            e.Property(x => x.PickLineId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.RequiredQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.PickedQty).HasColumnType("NUMERIC(18,4)");
        });

        b.Entity<Carton>(e =>
        {
            e.ToTable("Cartons", "wms");
            e.HasKey(x => x.CartonId);
            e.Property(x => x.CartonId).UseIdentityColumn();
            e.Property(x => x.CartonCode).HasMaxLength(80).IsRequired();
            e.Property(x => x.GrossWeightKg).HasColumnType("NUMERIC(10,2)");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedBy).HasMaxLength(450);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            e.Property(x => x.DeletedBy).HasMaxLength(450);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasMany(x => x.Contents)
                .WithOne()
                .HasForeignKey(l => l.CartonId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetField("_contents");
            e.Navigation(x => x.Contents)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<CartonContent>(e =>
        {
            e.ToTable("CartonContents", "wms");
            e.HasKey(x => x.ContentId);
            e.Property(x => x.ContentId).UseIdentityColumn();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.PackedQty).HasColumnType("NUMERIC(18,4)");

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .HasPrincipalKey(p => p.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── iot ───────────────────────────────────────────────────────────────
    private static void ConfigureIotSchema(ModelBuilder b)
    {
        b.Entity<AdapterInstance>(e =>
        {
            e.ToTable("AdapterInstances", "iot");
            e.HasKey(x => x.AdapterID);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ConfigJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.AdapterType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.WebhookApiKey).HasMaxLength(64);
            e.HasQueryFilter(x => !x.IsDeleted);
            e.HasMany(x => x.Signals).WithOne(s => s.Adapter).HasForeignKey(s => s.AdapterID);
            e.Navigation(x => x.Signals)
                .HasField("_signals")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<SignalMapping>(e =>
        {
            e.ToTable("SignalMappings", "iot");
            e.HasKey(x => x.SignalID);
            e.Property(x => x.AdapterID).IsRequired();
            e.Property(x => x.TagKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.SourceAddress).HasMaxLength(500).IsRequired();
            e.HasQueryFilter(x => !x.IsDeleted);
            e.HasIndex(x => new { x.AdapterID, x.TagKey }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        b.Entity<MachineStateRule>(e =>
        {
            e.ToTable("MachineStateRules", "iot");
            e.HasKey(x => x.RuleID);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.TargetState).HasMaxLength(20).IsRequired();
            e.Property(x => x.SignalTagKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.Operator).HasMaxLength(10).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<SignalTag>(e =>
        {
            e.ToTable("SignalTags", "iot");
            e.HasKey(x => x.TagId);
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.Property(x => x.DataType).HasMaxLength(20).IsRequired();
            e.Property(x => x.DefaultUnit).HasMaxLength(30);
            e.Property(x => x.TypicalMin).HasColumnType("decimal(18,4)");
            e.Property(x => x.TypicalMax).HasColumnType("decimal(18,4)");
            e.Property(x => x.Description).HasMaxLength(500);
        });

        b.Entity<MachineSignalLog>(e =>
        {
            e.ToTable("MachineSignalLogs", "iot");
            e.HasKey(x => x.LogId);
            e.Property(x => x.LogId).UseIdentityColumn();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.TagKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.Value).HasColumnType("decimal(18,6)");
            e.Property(x => x.Unit).HasMaxLength(30);
            e.Property(x => x.Source).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.MachineCode, x.TagKey, x.Timestamp });
        });

        b.Entity<MachineStateSnapshot>(e =>
        {
            e.ToTable("MachineStateSnapshots", "iot");
            e.HasKey(x => x.MachineCode);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.CurrentState).HasMaxLength(20).IsRequired();
            e.Property(x => x.PreviousState).HasMaxLength(20);
            e.Property(x => x.TriggerTagKey).HasMaxLength(100);
            e.Property(x => x.TriggerValue).HasColumnType("decimal(18,4)");
        });

        b.Entity<MachineStateHistory>(e =>
        {
            e.ToTable("MachineStateHistories", "iot");
            e.HasKey(x => x.HistoryId);
            e.Property(x => x.HistoryId).UseIdentityColumn();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.FromState).HasMaxLength(20).IsRequired();
            e.Property(x => x.ToState).HasMaxLength(20).IsRequired();
            e.Property(x => x.TriggerTagKey).HasMaxLength(100);
            e.Property(x => x.TriggerValue).HasColumnType("decimal(18,4)");
            e.HasIndex(x => new { x.MachineCode, x.TransitionAt });
        });

        b.Entity<SignalAgg1min>(e =>
        {
            e.ToTable("SignalAgg_1min", "iot");
            e.HasKey(x => x.BucketId);
            e.Property(x => x.BucketId).UseIdentityColumn();
            // Unique index on (MachineCode, TagKey, BucketAt) — also serves as the lookup index
            e.HasIndex(x => new { x.MachineCode, x.TagKey, x.BucketAt }).IsUnique();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.TagKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.SumValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.MinValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.MaxValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.LastValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.FirstValue).HasColumnType("decimal(18,4)");
        });

        b.Entity<SignalAgg1hr>(e =>
        {
            e.ToTable("SignalAgg_1hr", "iot");
            e.HasKey(x => x.BucketId);
            e.Property(x => x.BucketId).UseIdentityColumn();
            // Unique index on (MachineCode, TagKey, BucketAt) — also serves as the lookup index
            e.HasIndex(x => new { x.MachineCode, x.TagKey, x.BucketAt }).IsUnique();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.TagKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.SumValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.MinValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.MaxValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.LastValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.FirstValue).HasColumnType("decimal(18,4)");
        });

        b.Entity<RetentionPolicy>(e =>
        {
            e.ToTable("RetentionPolicies", "iot");
            e.HasKey(x => x.PolicyId);
            e.Property(x => x.Scope).HasMaxLength(20).IsRequired();
            e.Property(x => x.ScopeValue).HasMaxLength(100);
        });

        b.Entity<AdapterHealth>(e =>
        {
            e.ToTable("AdapterHealths", "iot");
            e.HasKey(x => x.AdapterId);
            e.Property(x => x.AdapterId).ValueGeneratedNever();
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.AdapterType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.LastError).HasMaxLength(500);
            e.HasIndex(x => x.MachineCode);
        });

        b.Entity<AdapterHealthLog>(e =>
        {
            e.ToTable("AdapterHealthLogs", "iot");
            e.HasKey(x => x.EventId);
            e.Property(x => x.EventId).UseIdentityColumn();
            e.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Details).HasMaxLength(500);
            e.HasIndex(x => new { x.AdapterId, x.EventAt });
        });
    }

    // ── rules ──────────────────────────────────────────────────────────────
    private static void ConfigureRulesSchema(ModelBuilder b)
    {
        b.Entity<Rule>(e =>
        {
            e.ToTable("Rules", "rules");
            e.HasKey(x => x.RuleId);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.TriggerType).HasMaxLength(50).IsRequired();
            e.Property(x => x.TriggerConfig).HasColumnType("nvarchar(max)");
            e.Property(x => x.CreatedBy).HasMaxLength(256);
            e.HasMany(x => x.Conditions).WithOne().HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Actions).WithOne().HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<RuleCondition>(e =>
        {
            e.ToTable("RuleConditions", "rules");
            e.HasKey(x => x.ConditionId);
            e.Property(x => x.LogicOperator).HasMaxLength(5).IsRequired();
            e.Property(x => x.ConditionType).HasMaxLength(50).IsRequired();
            e.Property(x => x.ConditionConfig).HasColumnType("nvarchar(max)");
        });

        b.Entity<RuleAction>(e =>
        {
            e.ToTable("RuleActions", "rules");
            e.HasKey(x => x.ActionId);
            e.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
            e.Property(x => x.ActionConfig).HasColumnType("nvarchar(max)");
        });

        b.Entity<RuleExecutionLog>(e =>
        {
            e.ToTable("RuleExecutionLogs", "rules");
            e.HasKey(x => x.ExecutionId);
            e.Property(x => x.ExecutionId).UseIdentityColumn();
            e.Property(x => x.EvaluationResult).HasMaxLength(30).IsRequired();
            e.Property(x => x.ActionsExecuted).HasColumnType("nvarchar(max)");
            e.Property(x => x.ContextSnapshot).HasColumnType("nvarchar(max)");
            e.HasIndex(x => new { x.RuleId, x.TriggeredAt });
        });
    }

    // ── lab ───────────────────────────────────────────────────────────────
    private static void ConfigureLabSchema(ModelBuilder b)
    {
        b.Entity<TestMethod>(e =>
        {
            e.ToTable("TestMethods", "lab");
            e.HasKey(x => x.TestMethodId);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            e.Property(x => x.MeasurementType).HasMaxLength(20).IsRequired();
            e.Property(x => x.SpecMin).HasColumnType("decimal(18,4)");
            e.Property(x => x.SpecMax).HasColumnType("decimal(18,4)");
            e.Property(x => x.SpecNominal).HasColumnType("decimal(18,4)");
            e.Property(x => x.ReferenceStd).HasMaxLength(100);
            e.Property(x => x.InstrumentType).HasMaxLength(100);
        });

        b.Entity<TestPanel>(e =>
        {
            e.ToTable("TestPanels", "lab");
            e.HasKey(x => x.PanelId);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PanelId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TestPanelItem>(e =>
        {
            e.ToTable("TestPanelItems", "lab");
            e.HasKey(x => x.PanelItemId);
            e.Property(x => x.SpecOverrideMin).HasColumnType("decimal(18,4)");
            e.Property(x => x.SpecOverrideMax).HasColumnType("decimal(18,4)");
        });

        b.Entity<LabRequest>(e =>
        {
            e.ToTable("LabRequests", "lab");
            e.HasKey(x => x.RequestId);
            e.Property(x => x.RequestNo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.RequestNo).IsUnique();
            e.Property(x => x.RequestType).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LotNumber).HasMaxLength(100);
            e.Property(x => x.CustomerCode).HasMaxLength(30);
            e.Property(x => x.SampleQty).HasColumnType("decimal(18,4)");
            e.Property(x => x.SampleUnit).HasMaxLength(20).IsRequired();
            e.Property(x => x.SampleLocation).HasMaxLength(200);
            e.Property(x => x.RequestedBy).HasMaxLength(256).IsRequired();
            e.Property(x => x.AssignedTo).HasMaxLength(256);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => new { x.Status, x.RequestedAt });
        });

        b.Entity<LabSample>(e =>
        {
            e.ToTable("LabSamples", "lab");
            e.HasKey(x => x.SampleId);
            e.Property(x => x.SampleCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.SampleCode).IsUnique();
            e.Property(x => x.ConditionOnReceipt).HasMaxLength(30).IsRequired();
            e.Property(x => x.ReceivedBy).HasMaxLength(256).IsRequired();
            e.Property(x => x.StorageLocation).HasMaxLength(100);
            e.Property(x => x.DisposalMethod).HasMaxLength(30);
        });

        b.Entity<TestResult>(e =>
        {
            e.ToTable("TestResults", "lab");
            e.HasKey(x => x.ResultId);
            e.Property(x => x.MeasuredValue).HasColumnType("decimal(18,4)");
            e.Property(x => x.AttributeResult).HasMaxLength(10);
            e.Property(x => x.InstrumentCode).HasMaxLength(50);
            e.Property(x => x.TestedBy).HasMaxLength(256).IsRequired();
            e.Property(x => x.ReviewedBy).HasMaxLength(256);
            e.Property(x => x.Notes).HasMaxLength(255);
            e.HasIndex(x => new { x.RequestId, x.SampleId });
        });

        b.Entity<LabReport>(e =>
        {
            e.ToTable("LabReports", "lab");
            e.HasKey(x => x.ReportId);
            e.Property(x => x.ReportNo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.ReportNo).IsUnique();
            e.Property(x => x.OverallResult).HasMaxLength(15).IsRequired();
            e.Property(x => x.Conclusion).HasMaxLength(500).IsRequired();
            e.Property(x => x.IssuedBy).HasMaxLength(256).IsRequired();
            e.Property(x => x.CustomerCode).HasMaxLength(30);
            e.Property(x => x.DocumentUrl).HasMaxLength(500);
        });
    }

    // ── settings ──────────────────────────────────────────────────────────
    private static void ConfigureSettingsSchema(ModelBuilder b)
    {
        b.Entity<SystemOptions>(e =>
        {
            e.ToTable("SystemOptions", "settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.MaterialManagementType).HasMaxLength(20);
            e.Property(x => x.PurchaseOrderDefaultAllocationMethod).HasMaxLength(50);
            e.Property(x => x.DispatchSequentialWorkflowEnforcement).HasMaxLength(10);
            e.Property(x => x.QcTargetSelection).HasMaxLength(20);
            e.Property(x => x.UpdatedBy).HasMaxLength(450);
            // ERP integration settings
            e.Property(x => x.ErpBaseUrl).HasMaxLength(500);
            e.Property(x => x.ErpApiKey).HasMaxLength(200);
        });
    }
}
