using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionOrderRepository(AppDbContext db) : IProductionOrderRepository
{
    public Task<ProductionOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POID == id, ct);

    public Task<ProductionOrder?> GetByIdWithDetailsAsync(int id, CancellationToken ct) =>
        db.ProductionOrders
            .Include(x => x.MaterialLines)
            .Include(x => x.Stages)
            .FirstOrDefaultAsync(x => x.POID == id, ct);

    public Task<ProductionOrder?> GetByCodeAsync(string poCode, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POCode == poCode, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.AnyAsync(x => x.POID == id, ct);

    public async Task<bool> HasDownstreamDocumentsAsync(int id, CancellationToken ct)
        => await db.WorkOrders.AnyAsync(w => w.POID == id, ct);

    public async Task<IReadOnlyList<ProductionOrder>> GetBySoIdAsync(int soId, CancellationToken ct) =>
        await db.ProductionOrders.AsNoTracking()
            .Where(x => x.SOID == soId)
            .OrderBy(x => x.POCode)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductionOrder>> GetFilteredAsync(
        int? soId, string? poCode, string? productCode,
        ProductionOrderStatus? status, CancellationToken ct)
    {
        var q = db.ProductionOrders.AsNoTracking().AsQueryable();

        if (soId.HasValue)
            q = q.Where(x => x.SOID == soId.Value);
        if (poCode is not null)
            q = q.Where(x => x.POCode.Contains(poCode.ToUpperInvariant()));
        if (productCode is not null)
            q = q.Where(x => x.ProductCode.Contains(productCode.ToUpperInvariant()));
        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);

        return await q.OrderByDescending(x => x.POID).ToListAsync(ct);
    }

    public Task AddAsync(ProductionOrder entity, CancellationToken ct)
    {
        db.ProductionOrders.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(ProductionOrder entity) => db.ProductionOrders.Remove(entity);

    public Task<int> CountAsync(CancellationToken ct) =>
        db.ProductionOrders.CountAsync(ct);

    public async Task<string> NextPoCodeAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.ProductionOrders
            .CountAsync(x => x.CreatedAt.HasValue && x.CreatedAt.Value.Year == year, ct);
        return $"PO-{year}-{count + 1:D4}";
    }
}
