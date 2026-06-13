using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SalesOrderRepository(AppDbContext db) : ISalesOrderRepository
{
    public Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.SalesOrders.AsNoTracking().FirstOrDefaultAsync(x => x.SOID == id, ct);

    public Task<SalesOrder?> GetByCodeAsync(string soCode, CancellationToken ct) =>
        db.SalesOrders.FirstOrDefaultAsync(x => x.SOCode == soCode.ToUpperInvariant(), ct);

    public Task AddAsync(SalesOrder entity, CancellationToken ct)
    {
        db.SalesOrders.Add(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SalesOrder>> GetFilteredAsync(
        string? soCode, SalesOrderStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.SalesOrders.AsNoTracking().AsQueryable();

        if (soCode is not null)
            q = q.Where(x => x.SOCode.Contains(soCode.ToUpperInvariant()));
        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);
        if (from.HasValue)
            q = q.Where(x => x.OrderDate >= from.Value);
        if (to.HasValue)
            q = q.Where(x => x.OrderDate <= to.Value);

        return await q.OrderByDescending(x => x.OrderDate).ToListAsync(ct);
    }
}
