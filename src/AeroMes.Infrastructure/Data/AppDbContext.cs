using AeroMes.Application.Interfaces;
using AeroMes.Domain.Common;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.ValueObjects;
using AeroMes.Domain.Quality;
using AeroMes.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
    : IdentityDbContext<ApplicationUser>(options), IUnitOfWork
{
    // master schema
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Routing> Routings => Set<Routing>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

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
            await publisher.Publish(domainEvent, ct);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        ConfigureMasterSchema(b);
        ConfigureIntegrationSchema(b);
        ConfigureProdSchema(b);
        ConfigureQualSchema(b);
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
            e.HasIndex(x => x.WorkCenterCode).IsUnique();
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
        });

        b.Entity<Product>(e =>
        {
            e.ToTable("Products", "master");
            e.HasKey(x => x.ProductCode);
            e.Property(x => x.ProductCode).HasMaxLength(50).ValueGeneratedNever();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductUnit).HasMaxLength(20).IsRequired();
            e.Property(x => x.BarcodePattern).HasMaxLength(100);
        });

        b.Entity<BomItem>(e =>
        {
            e.ToTable("BOM", "master");
            e.HasKey(x => x.BomID);
            e.Property(x => x.ParentProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ChildProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.RequiredQty).HasColumnType("NUMERIC(18,4)");
            e.Property(x => x.ScrapFactor).HasColumnType("NUMERIC(5,2)");
            e.HasIndex(x => new { x.ParentProductCode, x.ChildProductCode }).IsUnique();

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
        });

        b.Entity<Routing>(e =>
        {
            e.ToTable("Routings", "master");
            e.HasKey(x => x.RoutingID);
            e.Property(x => x.RoutingCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.RoutingName).HasMaxLength(150).IsRequired();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoutingCode).IsUnique();

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
            e.HasIndex(x => x.LocationCode).IsUnique();

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID)
                .IsRequired(false);
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
}
