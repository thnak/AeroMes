using AeroMes.Domain.Production;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InlineInspectionRepository(AppDbContext db) : IInlineInspectionRepository
{
    public Task AddAsync(InlineInspection inspection, CancellationToken ct = default)
    {
        db.InlineInspections.Add(inspection);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<DHUTrendDto>> GetDHUTrendAsync(
        int? woid, int? workCenterId, string? styleCode, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        var q = db.InlineInspections.AsNoTracking()
            .Where(i => i.InspectedAt >= fromDate && i.InspectedAt <= toDate);
        if (woid.HasValue) q = q.Where(i => i.WOID == woid.Value);
        if (workCenterId.HasValue) q = q.Where(i => i.WorkCenterID == workCenterId.Value);
        if (!string.IsNullOrWhiteSpace(styleCode))
            q = q.Where(i => i.StyleCode == styleCode.Trim().ToUpperInvariant());

        return await q
            .GroupBy(i => new
            {
                Period = new DateTime(i.InspectedAt.Year, i.InspectedAt.Month, i.InspectedAt.Day),
                i.StyleCode,
                i.WorkCenterID,
                i.ShiftCode,
            })
            .Select(g => new DHUTrendDto(
                g.Key.Period,
                g.Key.StyleCode,
                g.Key.WorkCenterID,
                g.Key.ShiftCode,
                g.Average(i => i.DHU),
                g.Count(),
                g.Count(i => i.IsAboveTarget)))
            .OrderBy(x => x.Period)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DefectParetoDto>> GetDefectParetoAsync(
        string? styleCode, int? workCenterId, DateTime fromDate, DateTime toDate, int topN,
        CancellationToken ct = default)
    {
        var q = db.InlineInspections.AsNoTracking()
            .Where(i => i.InspectedAt >= fromDate && i.InspectedAt <= toDate);
        if (workCenterId.HasValue) q = q.Where(i => i.WorkCenterID == workCenterId.Value);
        if (!string.IsNullOrWhiteSpace(styleCode))
            q = q.Where(i => i.StyleCode == styleCode.Trim().ToUpperInvariant());

        var defectQuery = q.SelectMany(i => i.Defects);

        var total = await defectQuery.SumAsync(d => (long)d.Quantity, ct);
        if (total == 0) return [];

        var rows = await defectQuery
            .GroupBy(d => new { d.DefectCode, d.IsMajor })
            .Select(g => new
            {
                g.Key.DefectCode,
                g.Key.IsMajor,
                TotalQty = g.Sum(d => d.Quantity),
                OccurrenceCount = g.Count(),
            })
            .OrderByDescending(x => x.TotalQty)
            .Take(topN)
            .Join(db.DefectCodes.AsNoTracking(),
                d => d.DefectCode,
                dc => dc.Code,
                (d, dc) => new DefectParetoDto(
                    d.DefectCode,
                    dc.DefectCategory,
                    d.IsMajor,
                    d.TotalQty,
                    d.OccurrenceCount,
                    (decimal)d.TotalQty / total * 100m))
            .ToListAsync(ct);

        return rows;
    }

    public async Task<QualitySummaryByStyleDto?> GetQualitySummaryAsync(
        string styleCode, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var code = styleCode.Trim().ToUpperInvariant();
        var inlineStats = await db.InlineInspections.AsNoTracking()
            .Where(i => i.StyleCode == code && i.InspectedAt >= fromDate && i.InspectedAt <= toDate)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                AvgDHU = g.Average(i => i.DHU),
                Count = g.Count(),
                AboveCount = g.Count(i => i.IsAboveTarget),
            })
            .FirstOrDefaultAsync(ct);

        if (inlineStats is null) return null;

        var styleWoids = await db.CutOrders.AsNoTracking()
            .Where(co => co.StyleCode == code)
            .Select(co => co.WOID)
            .Distinct()
            .ToListAsync(ct);

        var aqlStats = styleWoids.Count == 0 ? null :
            await db.AQLInspections.AsNoTracking()
                .Where(a => styleWoids.Contains(a.WOID)
                            && a.InspectedAt >= fromDate && a.InspectedAt <= toDate)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    AcceptCount = g.Count(a => a.Decision == "ACCEPT"),
                    RejectCount = g.Count(a => a.Decision == "REJECT"),
                })
                .FirstOrDefaultAsync(ct);

        var topDefects = await GetDefectParetoAsync(styleCode, null, fromDate, toDate, 3, ct);

        return new QualitySummaryByStyleDto(
            code,
            inlineStats.AvgDHU,
            inlineStats.Count,
            inlineStats.AboveCount,
            aqlStats?.Count ?? 0,
            aqlStats?.AcceptCount ?? 0,
            aqlStats?.RejectCount ?? 0,
            topDefects);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
