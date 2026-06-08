using AeroMes.Application.Interfaces;
using AeroMes.Domain.Entities;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
    public DbSet<DefectCode> DefectCodes => Set<DefectCode>();
    public DbSet<DefectDetail> DefectDetails => Set<DefectDetail>();
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();

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
            e.HasOne(x => x.WorkCenter)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.WorkCenterID);
        });

        modelBuilder.Entity<ProductionLog>(e =>
        {
            e.HasKey(x => x.LogID);
            e.Property(x => x.OperatorID).HasMaxLength(50).IsRequired();
            e.Property(x => x.MachineCode).HasMaxLength(50);
            e.Property(x => x.ShiftCode).HasMaxLength(20);
            e.Property(x => x.IdempotencyKey).HasMaxLength(36);
            e.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
            e.HasIndex(x => x.WorkOrderID);
            e.HasIndex(x => x.Timestamp);
            e.HasOne(x => x.WorkOrder)
                .WithMany(x => x.ProductionLogs)
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
            e.HasOne(x => x.ProductionLog)
                .WithMany(x => x.DefectDetails)
                .HasForeignKey(x => x.LogID);
            e.HasOne(x => x.DefectCode)
                .WithMany(x => x.DefectDetails)
                .HasForeignKey(x => x.DefectCodeID);
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
            e.HasOne(x => x.WorkCenter)
                .WithMany(x => x.DowntimeLogs)
                .HasForeignKey(x => x.WorkCenterID);
        });
    }
}
