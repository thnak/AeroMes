using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Services;

public class LotAllocationService(AppDbContext db) : ILotAllocationService
{
    public async Task<AllocationResult> AllocateAsync(
        string productCode,
        decimal requiredQty,
        int? locationId,
        PickingStrategy? strategyOverride,
        CancellationToken ct = default)
    {
        var code = productCode.Trim().ToUpperInvariant();

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.ProductCode == code)
            .Select(p => new { p.PickingStrategy, p.MinShelfLifeDaysOnIssue })
            .FirstOrDefaultAsync(ct);

        var strategy = strategyOverride ?? product?.PickingStrategy ?? PickingStrategy.Fefo;
        var minShelfLife = product?.MinShelfLifeDaysOnIssue;

        var stockQuery = db.InventoryStocks
            .AsNoTracking()
            .Where(s => s.ProductCode == code && s.Quantity > 0);

        if (locationId.HasValue)
            stockQuery = stockQuery.Where(s => s.LocationID == locationId.Value);

        // Exclude quarantine bins
        stockQuery = stockQuery.Where(s =>
            s.BinId == null ||
            !db.Set<Bin>().Any(b => b.BinId == s.BinId && b.BinType == BinType.Quarantine));

        var candidates = await stockQuery
            .Select(s => new StockCandidate(
                s.LotNumber,
                s.LocationID,
                s.BinId,
                s.Quantity,
                s.ExpiryDate,
                s.ReceivedAt))
            .ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minExpiryDate = minShelfLife.HasValue
            ? today.AddDays(minShelfLife.Value)
            : (DateOnly?)null;

        var accepted = new List<StockCandidate>();
        var rejected = new List<RejectedLot>();

        foreach (var c in candidates)
        {
            if (minExpiryDate.HasValue && c.ExpiryDate.HasValue && c.ExpiryDate.Value < minExpiryDate.Value)
            {
                rejected.Add(new RejectedLot(c.LotNumber, c.AvailableQty, c.ExpiryDate,
                    $"Lot expires {c.ExpiryDate:yyyy-MM-dd}, minimum required shelf life is {minShelfLife} days."));
            }
            else if (c.ExpiryDate.HasValue && c.ExpiryDate.Value < today)
            {
                rejected.Add(new RejectedLot(c.LotNumber, c.AvailableQty, c.ExpiryDate,
                    $"Lot expired on {c.ExpiryDate:yyyy-MM-dd}."));
            }
            else
            {
                accepted.Add(c);
            }
        }

        IEnumerable<StockCandidate> sorted = strategy switch
        {
            PickingStrategy.Fefo => accepted
                .OrderBy(c => c.ExpiryDate.HasValue ? 0 : 1)
                .ThenBy(c => c.ExpiryDate ?? DateOnly.MaxValue),
            PickingStrategy.Fifo => accepted.OrderBy(c => c.ReceivedAt),
            PickingStrategy.Lifo => accepted.OrderByDescending(c => c.ReceivedAt),
            _ => accepted,
        };

        var allocations = new List<LotAllocation>();
        var remaining = requiredQty;

        foreach (var candidate in sorted)
        {
            if (remaining <= 0) break;

            var take = Math.Min(candidate.AvailableQty, remaining);
            allocations.Add(new LotAllocation(
                candidate.LotNumber,
                candidate.LocationId,
                candidate.BinId,
                take,
                candidate.ExpiryDate));
            remaining -= take;
        }

        var allocated = requiredQty - remaining;

        return new AllocationResult(
            allocations,
            remaining <= 0,
            allocated,
            requiredQty,
            rejected);
    }

    private record StockCandidate(
        string LotNumber,
        int LocationId,
        int? BinId,
        decimal AvailableQty,
        DateOnly? ExpiryDate,
        DateTime ReceivedAt);
}
