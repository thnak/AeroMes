using AeroMes.Application.Interfaces;
using AeroMes.Domain.Common;
using AeroMes.Domain.Equipment;
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
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
    public DbSet<DefectDetail> DefectDetails => Set<DefectDetail>();
    public DbSet<DefectCode> DefectCodes => Set<DefectCode>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkCenter>(e =>
        {
            e.HasKey(x => x.WorkCenterID);
            e.Property(x => x.WorkCenterCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.WorkCenterName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.WorkCenterCode).IsUnique();
        });

        modelBuilder.Entity<WorkOrder>(e =>
        {
            e.HasKey(x => x.WorkOrderID);
            e.Property(x => x.WorkOrderNo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.WorkOrderNo).IsUnique();
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            e.Property(x => x.PlannedQty)
                .HasConversion(q => q.Value, v => Quantity.From(v));
            e.Property(x => x.ActualQtyOK)
                .HasConversion(q => q.Value, v => Quantity.From(v));
            e.Property(x => x.ActualQtyNG)
                .HasConversion(q => q.Value, v => Quantity.From(v));

            e.Property(x => x.RowVersion).IsRowVersion();

            e.HasOne(x => x.WorkCenter)
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID);
        });

        modelBuilder.Entity<ProductionLog>(e =>
        {
            e.HasKey(x => x.LogID);
            e.Property(x => x.OperatorID).HasMaxLength(50).IsRequired();
            e.Property(x => x.MachineCode).HasMaxLength(50);
            e.Property(x => x.ShiftCode).HasMaxLength(20);
            e.Property(x => x.IdempotencyKey).HasMaxLength(36);
            e.HasIndex(x => x.IdempotencyKey).IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
            e.HasIndex(x => x.WorkOrderID);
            e.HasIndex(x => x.Timestamp);

            e.Navigation(x => x.DefectDetails)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasOne<WorkOrder>()
                .WithMany()
                .HasForeignKey(x => x.WorkOrderID);
        });

        modelBuilder.Entity<DefectCode>(e =>
        {
            e.HasKey(x => x.DefectCodeID);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.DefectName).HasMaxLength(150).IsRequired();
            e.Property(x => x.DefectCategory).HasMaxLength(100);
        });

        modelBuilder.Entity<DefectDetail>(e =>
        {
            e.HasKey(x => x.DefectDetailID);
            e.HasIndex(x => x.LogID);

            e.HasOne(x => x.DefectCode)
                .WithMany()
                .HasForeignKey(x => x.DefectCodeID);

            e.HasOne<ProductionLog>()
                .WithMany("_defectDetails")
                .HasForeignKey(x => x.LogID);
        });

        modelBuilder.Entity<DowntimeLog>(e =>
        {
            e.HasKey(x => x.DowntimeLogID);
            e.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReasonName).HasMaxLength(150);
            e.Property(x => x.OperatorID).HasMaxLength(50);
            e.Ignore(x => x.DurationMinutes);
            e.HasIndex(x => x.WorkCenterID);
            e.HasIndex(x => x.StartTime);

            e.HasOne<WorkCenter>()
                .WithMany()
                .HasForeignKey(x => x.WorkCenterID);
        });
    }
}
