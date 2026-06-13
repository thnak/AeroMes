using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ScrapTransactionRepository(AppDbContext db) : IScrapTransactionRepository
{
    public Task AddAsync(ScrapTransaction tx, CancellationToken ct)
    {
        db.ScrapTransactions.Add(tx);
        return Task.CompletedTask;
    }

    public Task<ScrapTransaction?> GetByIdAsync(long id, CancellationToken ct)
        => db.ScrapTransactions.FirstOrDefaultAsync(x => x.ScrapTxID == id, ct);

    public async Task<(IReadOnlyList<ScrapTransactionDto> Items, int Total)> GetListAsync(
        int? woid, string? productCode, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct)
    {
        var q = db.ScrapTransactions.AsNoTracking();
        if (woid.HasValue) q = q.Where(x => x.WOID == woid.Value);
        if (!string.IsNullOrEmpty(productCode)) q = q.Where(x => x.ProductCode == productCode);
        if (from.HasValue) q = q.Where(x => x.ScrapAt >= from.Value);
        if (to.HasValue) q = q.Where(x => x.ScrapAt <= to.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.ScrapAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ScrapTransactionDto(
                x.ScrapTxID, x.WOID, x.LogID, x.DefectCodeId,
                x.ProductCode, x.LotNumber, x.ScrapQty,
                x.MaterialCostPerUnit, x.LaborCostSunk, x.MachineCostSunk,
                x.TotalScrapCost, x.DisposalMethod.ToString(), x.ScrapAt, x.Notes))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<ScrapParetoDto>> GetParetoAsync(
        DateTime from, DateTime to, int? workCenterId, CancellationToken ct)
    {
        var q = db.ScrapTransactions.AsNoTracking()
            .Where(x => x.ScrapAt >= from && x.ScrapAt <= to);

        return await q
            .GroupBy(x => new { x.DefectCodeId })
            .Select(g => new ScrapParetoDto(
                g.Key.DefectCodeId,
                null,
                null,
                g.Sum(x => x.ScrapQty),
                g.Sum(x => x.TotalScrapCost)))
            .OrderByDescending(x => x.TotalCost)
            .ToListAsync(ct);
    }
}
