using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class StandardCostRepository(AppDbContext db) : IStandardCostRepository
{
    public Task<StandardCostHeader?> GetByIdAsync(int id, CancellationToken ct) =>
        db.StandardCostHeaders.FirstOrDefaultAsync(x => x.StdCostId == id, ct);

    public Task<StandardCostHeader?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.StandardCostHeaders
            .Include(x => x.MaterialLines)
            .Include(x => x.RoutingLines)
            .FirstOrDefaultAsync(x => x.StdCostId == id, ct);

    public Task<StandardCostHeader?> GetActiveByProductAsync(string productCode, CancellationToken ct) =>
        db.StandardCostHeaders.FirstOrDefaultAsync(
            x => x.ProductCode == productCode && x.Status == StandardCostStatus.Active, ct);

    public async Task<IReadOnlyList<StandardCostSummaryDto>> GetListAsync(
        string? productCode, string? status, CancellationToken ct)
    {
        var q = db.StandardCostHeaders.AsNoTracking().AsQueryable();
        if (productCode is not null)
            q = q.Where(x => x.ProductCode.Contains(productCode.ToUpperInvariant()));
        if (status is not null && Enum.TryParse<StandardCostStatus>(status, ignoreCase: true, out var s))
            q = q.Where(x => x.Status == s);

        return await q
            .OrderByDescending(x => x.StdCostId)
            .Select(x => new StandardCostSummaryDto(
                x.StdCostId, x.ProductCode, x.CostVersion, x.Status.ToString(),
                x.TotalMaterialCost, x.TotalLaborCost, x.TotalMachineCost, x.TotalOverheadCost,
                x.TotalMaterialCost + x.TotalLaborCost + x.TotalMachineCost + x.TotalOverheadCost,
                x.Currency, x.EffectiveFrom, x.EffectiveTo,
                x.CreatedBy, x.CalculatedAt))
            .ToListAsync(ct);
    }

    public async Task<int> NextVersionForProductAsync(string productCode, CancellationToken ct)
    {
        var max = await db.StandardCostHeaders
            .Where(x => x.ProductCode == productCode)
            .MaxAsync(x => (int?)x.CostVersion, ct);
        return (max ?? 0) + 1;
    }

    public Task AddAsync(StandardCostHeader entity, CancellationToken ct)
    {
        db.StandardCostHeaders.Add(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
