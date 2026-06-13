using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ModuleStatusRepository(AppDbContext db) : IModuleStatusRepository
{
    public Task<int> CountActiveWorkOrdersAsync(CancellationToken ct) =>
        db.WorkOrders.AsNoTracking()
            .CountAsync(x => x.Status == WorkOrderStatus.Running || x.Status == WorkOrderStatus.Paused, ct);

    public Task<int> CountOpenDowntimeLogsAsync(CancellationToken ct) =>
        db.DowntimeLogs.AsNoTracking()
            .CountAsync(x => x.EndTime == null, ct);
}
