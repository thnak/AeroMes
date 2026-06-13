using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class StockPolicyRepository(AppDbContext db) : IStockPolicyRepository
{
    public async Task<IReadOnlyList<StockPolicy>> GetAllAsync(bool? isActive, CancellationToken ct)
    {
        var q = db.StockPolicies.AsNoTracking().AsQueryable();
        if (isActive.HasValue)
            q = q.Where(p => p.IsActive == isActive.Value);
        return await q.OrderBy(p => p.ProductCode).ThenBy(p => p.LocationId).ToListAsync(ct);
    }

    public async Task<StockPolicy?> GetByIdAsync(int id, CancellationToken ct) =>
        await db.StockPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PolicyId == id, ct);

    public async Task<StockPolicy?> GetActiveByProductAndLocationAsync(
        string productCode, int locationId, CancellationToken ct) =>
        await db.StockPolicies
            .FirstOrDefaultAsync(p =>
                p.ProductCode == productCode.Trim().ToUpperInvariant() &&
                p.LocationId == locationId &&
                p.IsActive, ct);

    public async Task<bool> ExistsForProductAndLocationAsync(
        string productCode, int locationId, int? excludeId, CancellationToken ct) =>
        await db.StockPolicies.AsNoTracking()
            .AnyAsync(p =>
                p.ProductCode == productCode.Trim().ToUpperInvariant() &&
                p.LocationId == locationId &&
                (excludeId == null || p.PolicyId != excludeId.Value), ct);

    public async Task AddAsync(StockPolicy policy, CancellationToken ct) =>
        await db.StockPolicies.AddAsync(policy, ct);
}
