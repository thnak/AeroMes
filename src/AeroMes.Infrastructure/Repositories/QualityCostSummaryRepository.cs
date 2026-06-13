using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityCostSummaryRepository(AppDbContext db) : IQualityCostSummaryRepository
{
    public Task<QualityCostSummary?> GetByPeriodAsync(
        short year, byte month, string? productCode, int? workCenterId, CancellationToken ct)
        => db.QualityCostSummaries.FirstOrDefaultAsync(x =>
            x.PeriodYear == year && x.PeriodMonth == month
            && x.ProductCode == productCode && x.WorkCenterID == workCenterId, ct);

    public Task AddAsync(QualityCostSummary summary, CancellationToken ct)
    {
        db.QualityCostSummaries.Add(summary);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<QualityCostSummaryDto>> GetSummaryAsync(
        short year, byte? month, string? productCode, CancellationToken ct)
    {
        var q = db.QualityCostSummaries.AsNoTracking().Where(x => x.PeriodYear == year);
        if (month.HasValue) q = q.Where(x => x.PeriodMonth == month.Value);
        if (!string.IsNullOrEmpty(productCode)) q = q.Where(x => x.ProductCode == productCode);

        return await q
            .OrderBy(x => x.PeriodMonth)
            .Select(x => new QualityCostSummaryDto(
                x.SummaryID, x.PeriodYear, x.PeriodMonth, x.ProductCode, x.WorkCenterID,
                x.PreventionCost, x.AppraisalCost, x.InternalScrapCost, x.ReworkCost,
                x.CustomerReturnCost, x.WarrantyCost, x.TotalQualityCost,
                x.TotalProductionValue, x.CopqPct, x.LastRefreshedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CopqTrendPointDto>> GetCopqTrendAsync(
        int months, string? productCode, CancellationToken ct)
    {
        var q = db.QualityCostSummaries.AsNoTracking()
            .Where(x => x.ProductCode == productCode)
            .OrderByDescending(x => x.PeriodYear)
            .ThenByDescending(x => x.PeriodMonth)
            .Take(months);

        return await q
            .Select(x => new CopqTrendPointDto(x.PeriodYear, x.PeriodMonth, x.ProductCode, x.TotalQualityCost, x.CopqPct))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);
    }
}
