using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class BundleRepository(AppDbContext db) : IBundleRepository
{
    public Task AddRangeAsync(IEnumerable<Bundle> bundles, CancellationToken ct = default)
    {
        db.Bundles.AddRange(bundles);
        return Task.CompletedTask;
    }

    public Task<Bundle?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
        => db.Bundles.FirstOrDefaultAsync(x => x.BundleBarcode == barcode.Trim().ToUpperInvariant(), ct);

    public Task<Bundle?> GetByIdAsync(int bundleId, CancellationToken ct = default)
        => db.Bundles.FirstOrDefaultAsync(x => x.BundleID == bundleId, ct);

    public Task<BundleMovement?> GetOpenMovementAsync(int bundleId, CancellationToken ct = default)
        => db.BundleMovements.FirstOrDefaultAsync(x => x.BundleID == bundleId && x.EndTime == null, ct);

    public Task AddMovementAsync(BundleMovement movement, CancellationToken ct = default)
    {
        db.BundleMovements.Add(movement);
        return Task.CompletedTask;
    }

    public async Task<BundleLocationDto?> GetLocationAsync(string barcode, CancellationToken ct = default)
    {
        var upper = barcode.Trim().ToUpperInvariant();
        return await db.Bundles.AsNoTracking()
            .Where(b => b.BundleBarcode == upper)
            .Select(b => new BundleLocationDto(
                b.BundleID, b.BundleBarcode, b.CutOrderID, b.StyleCode, b.ColorCode,
                b.SizeCode, b.Quantity, b.CurrentOperationCode, b.CurrentWorkCenterID,
                b.Status.ToString(), b.QtyOKCumulative, b.QtyNGCumulative, b.CreatedAt,
                db.BundleMovements
                    .Where(m => m.BundleID == b.BundleID && m.EndTime == null)
                    .Select(m => (string?)m.OperatorID).FirstOrDefault(),
                db.BundleMovements
                    .Where(m => m.BundleID == b.BundleID && m.EndTime == null)
                    .Select(m => (DateTime?)m.StartTime).FirstOrDefault()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<WIPByStyleDto>> GetWIPByStyleAsync(
        string styleCode, string? colorCode, int? woid, CancellationToken ct = default)
    {
        var code = styleCode.Trim().ToUpperInvariant();
        var q = db.Bundles.AsNoTracking().Where(b => b.StyleCode == code);
        if (!string.IsNullOrWhiteSpace(colorCode))
            q = q.Where(b => b.ColorCode == colorCode.Trim().ToUpperInvariant());
        if (woid.HasValue)
            q = q.Where(b => db.CutOrders.Any(co => co.CutOrderID == b.CutOrderID && co.WOID == woid.Value));

        return await q
            .GroupBy(b => new { b.StyleCode, b.ColorCode, b.SizeCode, b.CurrentOperationCode, b.Status })
            .Select(g => new WIPByStyleDto(
                g.Key.StyleCode, g.Key.ColorCode, g.Key.SizeCode,
                g.Key.CurrentOperationCode, g.Key.Status.ToString(),
                g.Count(), g.Sum(b => b.Quantity)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LineBalancingDto>> GetLineBalancingAsync(
        int? workCenterId, string? styleCode, CancellationToken ct = default)
    {
        var q = db.Bundles.AsNoTracking()
            .Where(b => b.Status != BundleStatus.Packed);
        if (workCenterId.HasValue)
            q = q.Where(b => b.CurrentWorkCenterID == workCenterId.Value);
        if (!string.IsNullOrWhiteSpace(styleCode))
            q = q.Where(b => b.StyleCode == styleCode.Trim().ToUpperInvariant());

        return await q
            .GroupBy(b => new { b.CurrentOperationCode, b.CurrentWorkCenterID })
            .Select(g => new LineBalancingDto(
                g.Key.CurrentOperationCode ?? "UNASSIGNED",
                g.Key.CurrentWorkCenterID,
                g.Count(), g.Sum(b => b.Quantity)))
            .OrderByDescending(x => x.BundleCount)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OperatorEfficiencyDto>> GetOperatorEfficiencyAsync(
        string? operatorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var q = db.BundleMovements.AsNoTracking()
            .Where(m => m.EndTime != null);
        if (!string.IsNullOrWhiteSpace(operatorId))
            q = q.Where(m => m.OperatorID == operatorId.Trim());
        if (fromDate.HasValue) q = q.Where(m => m.StartTime >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(m => m.StartTime <= toDate.Value);

        return await q
            .GroupBy(m => new { m.OperatorID, m.OperationCode })
            .Select(g => new OperatorEfficiencyDto(
                g.Key.OperatorID,
                g.Key.OperationCode,
                g.Count(),
                g.Average(m => m.EfficiencyPct),
                g.Average(m => m.ActualMinsPerPiece)))
            .OrderBy(x => x.OperatorID)
            .ThenBy(x => x.OperationCode)
            .ToListAsync(ct);
    }

    public async Task<int> GetConsecutiveLowEfficiencyCountAsync(
        string operatorId, string operationCode, decimal threshold, CancellationToken ct = default)
    {
        var recentMovements = await db.BundleMovements.AsNoTracking()
            .Where(m => m.OperatorID == operatorId && m.OperationCode == operationCode
                        && m.EndTime != null && m.EfficiencyPct != null)
            .OrderByDescending(m => m.EndTime)
            .Take(5)
            .Select(m => m.EfficiencyPct)
            .ToListAsync(ct);

        var count = 0;
        foreach (var eff in recentMovements)
        {
            if (eff < threshold) count++;
            else break;
        }
        return count;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
