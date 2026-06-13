using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CutOrderRepository(AppDbContext db) : ICutOrderRepository
{
    public Task AddAsync(CutOrder cutOrder, CancellationToken ct = default)
    {
        db.CutOrders.Add(cutOrder);
        return Task.CompletedTask;
    }

    public Task<CutOrder?> GetByIdAsync(int cutOrderId, CancellationToken ct = default)
        => db.CutOrders
            .Include(x => x.Lines)
            .Include(x => x.FabricUsage)
            .FirstOrDefaultAsync(x => x.CutOrderID == cutOrderId, ct);

    public async Task<CutOrderDto?> GetDetailAsync(int cutOrderId, CancellationToken ct = default)
    {
        var order = await db.CutOrders.AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.FabricUsage)
            .FirstOrDefaultAsync(x => x.CutOrderID == cutOrderId, ct);

        return order is null ? null : ToDto(order);
    }

    public async Task<IReadOnlyList<CutOrderDto>> GetByWOAsync(int woid, CancellationToken ct = default)
    {
        var orders = await db.CutOrders.AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.FabricUsage)
            .Where(x => x.WOID == woid)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return orders.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<MarkerEfficiencyReportDto>> GetMarkerEfficiencyReportAsync(
        DateTime? fromDate, DateTime? toDate, string? styleCode, CancellationToken ct = default)
    {
        var q = db.CutOrders.AsNoTracking()
            .Where(x => x.Status == CutOrderStatus.Completed);

        if (fromDate.HasValue) q = q.Where(x => x.CuttingCompletedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(x => x.CuttingCompletedAt <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(styleCode))
            q = q.Where(x => x.StyleCode == styleCode.Trim().ToUpperInvariant());

        return await q
            .OrderBy(x => x.MarkerEfficiencyPct)
            .Select(x => new MarkerEfficiencyReportDto(
                x.StyleCode,
                x.ColorCode,
                x.CutOrderID,
                x.CutOrderCode,
                x.MarkerEfficiencyPct,
                x.FabricWastePct,
                x.CuttingCompletedAt))
            .ToListAsync(ct);
    }

    public Task<bool> CodeExistsAsync(string cutOrderCode, CancellationToken ct = default)
        => db.CutOrders.AnyAsync(x => x.CutOrderCode == cutOrderCode.Trim().ToUpperInvariant(), ct);

    public Task AddBundlesAsync(IEnumerable<Bundle> bundles, CancellationToken ct = default)
    {
        db.Bundles.AddRange(bundles);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    private static CutOrderDto ToDto(CutOrder o)
        => new(
            o.CutOrderID, o.CutOrderCode, o.WOID, o.StyleCode, o.ColorCode,
            o.FabricProductCode, o.ShadeCode, o.MarkerReference, o.MarkerEfficiencyPct,
            o.PlyCount, o.SpreadLengthMeters, o.FabricWidthCm, o.EstimatedFabricMeters,
            o.ActualFabricMeters, o.FabricWastePct, o.Status.ToString(),
            o.CuttingStartedAt, o.CuttingCompletedAt, o.CreatedAt,
            o.Lines.Select(l => new CutOrderLineDto(l.LineID, l.SizeCode, l.QuantityToCut, l.QuantityCut))
                   .ToList(),
            o.FabricUsage.Select(u => new CutOrderFabricUsageDto(u.UsageID, u.RollID, u.MetersUsed))
                         .ToList());
}
