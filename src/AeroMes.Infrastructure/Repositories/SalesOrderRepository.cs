using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SalesOrderRepository(AppDbContext db) : ISalesOrderRepository
{
    public Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.SalesOrders.FirstOrDefaultAsync(x => x.SOID == id, ct);

    public Task<SalesOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.SOID == id, ct);

    public Task<SalesOrder?> GetByCodeAsync(string soCode, CancellationToken ct) =>
        db.SalesOrders.FirstOrDefaultAsync(x => x.SOCode == soCode.ToUpperInvariant(), ct);

    public async Task<string> NextSoCodeAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.SalesOrders
            .CountAsync(x => x.CreatedAt.Year == year, ct);
        return $"SO-{year}-{count + 1:D4}";
    }

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

    public async Task<IReadOnlyList<SalesOrderSummaryDto>> GetListAsync(
        string? soCode, string? status, bool includeUnconfirmed,
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.SalesOrders.AsNoTracking().AsQueryable();

        if (!includeUnconfirmed)
            q = q.Where(x => x.Status != SalesOrderStatus.Unconfirmed);
        if (soCode is not null)
            q = q.Where(x => x.SOCode.Contains(soCode.ToUpperInvariant()));
        if (status is not null && Enum.TryParse<SalesOrderStatus>(status, ignoreCase: true, out var s))
            q = q.Where(x => x.Status == s);
        if (from.HasValue)
            q = q.Where(x => x.OrderDate >= from.Value);
        if (to.HasValue)
            q = q.Where(x => x.OrderDate <= to.Value);

        return await q
            .OrderByDescending(x => x.SOID)
            .Select(x => new SalesOrderSummaryDto(
                x.SOID, x.SOCode, x.CustomerCode, x.CustomerName,
                x.OrderDate, x.DeliveryDate,
                x.Status.ToString(), x.SyncSource.ToString(),
                x.FacilityCode,
                x.ConfirmedAt != null,
                x.CreatedBy, x.CreatedAt))
            .ToListAsync(ct);
    }
}
