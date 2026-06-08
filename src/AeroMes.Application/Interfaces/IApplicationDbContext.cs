using AeroMes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<WorkCenter> WorkCenters { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<ProductionLog> ProductionLogs { get; }
    DbSet<DefectCode> DefectCodes { get; }
    DbSet<DefectDetail> DefectDetails { get; }
    DbSet<DowntimeLog> DowntimeLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
