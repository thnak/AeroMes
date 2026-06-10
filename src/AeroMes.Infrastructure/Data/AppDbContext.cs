using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Domain.Common;
using Microsoft.AspNetCore.Identity;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.ValueObjects;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Settings;
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
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Routing> Routings => Set<Routing>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<ShiftTemplate> ShiftTemplates => Set<ShiftTemplate>();
    public DbSet<DowntimeReasonCode> DowntimeReasonCodes => Set<DowntimeReasonCode>();
    public DbSet<MachineProductConfig> MachineProductConfigs => Set<MachineProductConfig>();
    public DbSet<AlertThreshold> AlertThresholds => Set<AlertThreshold>();
    public DbSet<WorkOrderAutoRules> WorkOrderAutoRules => Set<WorkOrderAutoRules>();

    // integration schema
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    // prod schema
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();

    // qual schema
    public DbSet<DefectCode> DefectCodes => Set<DefectCode>();
    public DbSet<DefectDetail> DefectDetails => Set<DefectDetail>();

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
        ConfigureQualSchema(b);
        ConfigureSettingsSchema(b);
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

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        b.Entity<Product>(e =>
        {
            e.ToTable("Products", "master");
            e.HasKey(x => x.ProductCode);
            e.Property(x => x.ProductCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductUnit).HasMaxLength(20).IsRequired();
            e.Property(x => x.BarcodePattern).HasMaxLength(100);
            e.HasQueryFilter(x => !x.IsDeleted);
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
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.SOCode).IsUnique();
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
            e.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
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
            e.HasIndex(x => new { x.LocationID, x.ProductCode, x.LotNumber }).IsUnique();
            e.HasIndex(x => new { x.ProductCode, x.LotNumber });

            e.HasOne(x => x.StorageLocation)
                .WithMany()
                .HasForeignKey(x => x.LocationID);

            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductCode)
                .OnDelete(DeleteBehavior.Restrict);
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
            e.HasIndex(x => x.Code).IsUnique();
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
        });
    }
}
