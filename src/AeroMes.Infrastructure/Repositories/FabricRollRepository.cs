using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class FabricRollRepository(AppDbContext db) : IFabricRollRepository
{
    public Task AddAsync(FabricRoll roll, CancellationToken ct = default)
    {
        db.FabricRolls.Add(roll);
        return Task.CompletedTask;
    }

    public Task<FabricRoll?> GetByIdAsync(int rollId, CancellationToken ct = default)
        => db.FabricRolls.FirstOrDefaultAsync(x => x.RollID == rollId, ct);

    public Task<FabricRoll?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
        => db.FabricRolls.FirstOrDefaultAsync(
            x => x.RollBarcode == barcode.Trim().ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<FabricRoll>> GetByIdsAsync(
        IReadOnlyList<int> rollIds, CancellationToken ct = default)
        => await db.FabricRolls.Where(x => rollIds.Contains(x.RollID)).ToListAsync(ct);

    public async Task<IReadOnlyList<FabricRollDto>> GetAvailableByProductAndShadeAsync(
        string fabricProductCode, string? shadeCode, CancellationToken ct = default)
    {
        var code = fabricProductCode.Trim().ToUpperInvariant();
        var q = db.FabricRolls.AsNoTracking()
            .Where(x => x.FabricProductCode == code && x.Status == FabricRollStatus.Available);
        if (!string.IsNullOrWhiteSpace(shadeCode))
            q = q.Where(x => x.ShadeCode == shadeCode.Trim().ToUpperInvariant());

        return await q
            .OrderBy(x => x.RemainingLengthMeters)
            .Select(x => new FabricRollDto(
                x.RollID, x.RollBarcode, x.FabricProductCode, x.LotNumber, x.ShadeCode,
                x.GrossLengthMeters, x.GrossWeightKg, x.RemainingLengthMeters, x.RemainingWeightKg,
                x.FabricWidthCm, x.SupplierCode, x.ReceivedDate, x.LocationID,
                x.Status.ToString(), x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<FabricRollDto?> GetDetailAsync(int rollId, CancellationToken ct = default)
        => await db.FabricRolls.AsNoTracking()
            .Where(x => x.RollID == rollId)
            .Select(x => new FabricRollDto(
                x.RollID, x.RollBarcode, x.FabricProductCode, x.LotNumber, x.ShadeCode,
                x.GrossLengthMeters, x.GrossWeightKg, x.RemainingLengthMeters, x.RemainingWeightKg,
                x.FabricWidthCm, x.SupplierCode, x.ReceivedDate, x.LocationID,
                x.Status.ToString(), x.CreatedAt))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<FabricConsumptionLogDto>> GetHistoryAsync(int rollId, CancellationToken ct = default)
        => await db.FabricConsumptionLogs.AsNoTracking()
            .Where(x => x.RollID == rollId)
            .OrderByDescending(x => x.RecordedAt)
            .Select(x => new FabricConsumptionLogDto(
                x.ConsumptionID, x.RollID, x.CutOrderID, x.MetersConsumed,
                x.RemainingAfter, x.RecordedAt, x.RecordedBy))
            .ToListAsync(ct);

    public Task AddConsumptionLogAsync(FabricConsumptionLog log, CancellationToken ct = default)
    {
        db.FabricConsumptionLogs.Add(log);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<FabricInventorySummaryDto>> GetInventorySummaryAsync(
        string? fabricProductCode, CancellationToken ct = default)
    {
        var q = db.FabricRolls.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(fabricProductCode))
            q = q.Where(x => x.FabricProductCode == fabricProductCode.Trim().ToUpperInvariant());

        var grouped = await q
            .GroupBy(x => new { x.FabricProductCode, x.ShadeCode })
            .Select(g => new
            {
                g.Key.FabricProductCode,
                g.Key.ShadeCode,
                TotalRolls = g.Count(),
                AvailableRolls = g.Count(x => x.Status == FabricRollStatus.Available),
                TotalMeters = g.Sum(x => x.RemainingLengthMeters),
                AvailableMeters = g.Where(x => x.Status == FabricRollStatus.Available)
                                   .Sum(x => x.RemainingLengthMeters),
                TotalWeightKg = g.Sum(x => x.RemainingWeightKg),
                AvailableWeightKg = g.Where(x => x.Status == FabricRollStatus.Available)
                                     .Sum(x => x.RemainingWeightKg),
            })
            .ToListAsync(ct);

        return grouped.Select(g => new FabricInventorySummaryDto(
            g.FabricProductCode, g.ShadeCode, g.TotalRolls, g.AvailableRolls,
            g.TotalMeters, g.AvailableMeters, g.TotalWeightKg, g.AvailableWeightKg))
        .ToList();
    }

    public Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default)
        => db.FabricRolls.AnyAsync(x => x.RollBarcode == barcode.Trim().ToUpperInvariant(), ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
