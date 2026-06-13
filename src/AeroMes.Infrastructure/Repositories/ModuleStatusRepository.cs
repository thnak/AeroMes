using AeroMes.Application.Interfaces;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Production;
using AeroMes.Domain.Wms;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ModuleStatusRepository(AppDbContext db) : IModuleStatusRepository
{
    public Task<int> CountActiveWorkOrdersAsync(CancellationToken ct) =>
        db.WorkOrders.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && (x.Status == WorkOrderStatus.Running || x.Status == WorkOrderStatus.Paused), ct);

    public Task<int> CountOpenDowntimeLogsAsync(CancellationToken ct) =>
        db.DowntimeLogs.AsNoTracking()
            .CountAsync(x => x.EndTime == null, ct);

    public Task<int> CountPendingGrnAsync(CancellationToken ct) =>
        db.GoodsReceiptNotes.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.Status == GrnStatus.Draft, ct);

    public Task<int> CountAdaptersDownAsync(CancellationToken ct) =>
        db.AdapterInstances.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.IsEnabled && x.Status == AdapterStatus.Disconnected, ct);
}
