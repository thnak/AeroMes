using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ItemCostHistoryRepository(AppDbContext db) : IItemCostHistoryRepository
{
    public async Task<int> AddAsync(ItemCostHistory cost, CancellationToken ct)
    {
        db.ItemCostHistories.Add(cost);
        await db.SaveChangesAsync(ct);
        return cost.CostID;
    }

    public Task<ItemCostHistory?> GetActiveAsync(string productCode, ItemCostType costType, CancellationToken ct)
        => db.ItemCostHistories
            .Where(c => c.ProductCode == productCode && c.CostType == costType && c.EffectiveTo == null)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ItemCostHistoryDto>> GetByProductAsync(
        string productCode, CancellationToken ct)
        => await db.ItemCostHistories.AsNoTracking()
            .Where(c => c.ProductCode == productCode)
            .OrderByDescending(c => c.EffectiveFrom)
            .Select(c => new ItemCostHistoryDto(
                c.CostID, c.ProductCode, c.CostType.ToString(),
                c.UnitCost, c.CostUoM, c.EffectiveFrom, c.EffectiveTo,
                c.SourceRef, c.ApprovedBy, c.CreatedAt))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
