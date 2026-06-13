using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MultiProductionOrderRepository(AppDbContext db) : IMultiProductionOrderRepository
{
    public Task<MultiProductionOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.MultiProductionOrders
            .Include(m => m.Lines)
            .FirstOrDefaultAsync(m => m.MPOId == id, ct);

    public Task<MultiProductionOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct) =>
        db.MultiProductionOrders
            .Include(m => m.Lines)
            .FirstOrDefaultAsync(m => m.OrderNumber == orderNumber.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<MultiProductionOrder>> GetFilteredAsync(
        MultiProductionOrderType? orderType,
        MultiProductionOrderStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken ct)
    {
        var q = db.MultiProductionOrders.AsNoTracking().Include(m => m.Lines).AsQueryable();

        if (orderType.HasValue)
            q = q.Where(m => m.OrderType == orderType.Value);
        if (status.HasValue)
            q = q.Where(m => m.Status == status.Value);
        if (from.HasValue)
            q = q.Where(m => m.PlannedStart >= from.Value || m.CreatedAt >= from.Value);
        if (to.HasValue)
            q = q.Where(m => m.PlannedEnd <= to.Value || m.CreatedAt <= to.Value);

        return await q.OrderByDescending(m => m.MPOId).ToListAsync(ct);
    }

    public Task AddAsync(MultiProductionOrder entity, CancellationToken ct)
    {
        db.MultiProductionOrders.Add(entity);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken ct) =>
        db.MultiProductionOrders.CountAsync(ct);
}
