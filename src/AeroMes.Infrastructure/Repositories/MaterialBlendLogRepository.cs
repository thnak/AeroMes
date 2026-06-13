using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialBlendLogRepository(AppDbContext db) : IMaterialBlendLogRepository
{
    public Task AddAsync(MaterialBlendLog blendLog, CancellationToken ct = default)
    {
        db.MaterialBlendLogs.Add(blendLog);
        return Task.CompletedTask;
    }

    public Task<MaterialBlendLog?> GetByIdAsync(long blendLogId, CancellationToken ct = default)
        => db.MaterialBlendLogs.FirstOrDefaultAsync(x => x.BlendLogID == blendLogId, ct);

    public async Task<IReadOnlyList<MaterialBlendLogDto>> GetByJobAsync(long jobId, CancellationToken ct = default)
    {
        return await db.MaterialBlendLogs
            .AsNoTracking()
            .Where(x => x.JobID == jobId)
            .OrderByDescending(x => x.RecordedAt)
            .Select(x => new MaterialBlendLogDto(
                x.BlendLogID,
                x.JobID,
                x.ResinProductCode,
                x.VirginLotNumber,
                x.VirginQtyKg,
                x.RegrindLotNumber,
                x.RegrindQtyKg,
                x.VirginQtyKg + x.RegrindQtyKg,
                (x.VirginQtyKg + x.RegrindQtyKg) > 0
                    ? (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m
                    : 0m,
                x.MaxAllowedPct,
                (x.VirginQtyKg + x.RegrindQtyKg) > 0
                    ? (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m <= x.MaxAllowedPct
                    : true,
                x.ApprovedBy,
                x.ApprovedAt,
                x.ApprovalNotes,
                x.RecordedAt))
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<MaterialBlendLogDto> Items, int Total)> GetNonCompliantAsync(
        DateTime? fromDate, DateTime? toDate, bool? isApproved, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.MaterialBlendLogs
            .AsNoTracking()
            .Where(x => (x.VirginQtyKg + x.RegrindQtyKg) > 0
                        && (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m > x.MaxAllowedPct);

        if (fromDate.HasValue) q = q.Where(x => x.RecordedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(x => x.RecordedAt <= toDate.Value);
        if (isApproved.HasValue)
            q = isApproved.Value
                ? q.Where(x => x.ApprovedBy != null)
                : q.Where(x => x.ApprovedBy == null);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MaterialBlendLogDto(
                x.BlendLogID,
                x.JobID,
                x.ResinProductCode,
                x.VirginLotNumber,
                x.VirginQtyKg,
                x.RegrindLotNumber,
                x.RegrindQtyKg,
                x.VirginQtyKg + x.RegrindQtyKg,
                (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m,
                x.MaxAllowedPct,
                false,
                x.ApprovedBy,
                x.ApprovedAt,
                x.ApprovalNotes,
                x.RecordedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<RegrindUsageSummaryDto>> GetSummaryAsync(
        string? productCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var q = db.MaterialBlendLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(productCode))
            q = q.Where(x => x.ResinProductCode == productCode.Trim().ToUpperInvariant());
        if (fromDate.HasValue) q = q.Where(x => x.RecordedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(x => x.RecordedAt <= toDate.Value);

        var grouped = await q
            .GroupBy(x => x.ResinProductCode)
            .Select(g => new
            {
                ResinProductCode = g.Key,
                TotalVirginKg = g.Sum(x => x.VirginQtyKg),
                TotalRegrindKg = g.Sum(x => x.RegrindQtyKg),
                TotalBlendedKg = g.Sum(x => x.VirginQtyKg + x.RegrindQtyKg),
                TotalBlends = g.Count(),
                NonCompliantBlends = g.Count(x =>
                    (x.VirginQtyKg + x.RegrindQtyKg) > 0
                    && (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m > x.MaxAllowedPct),
                ApprovedNonCompliantBlends = g.Count(x =>
                    (x.VirginQtyKg + x.RegrindQtyKg) > 0
                    && (x.RegrindQtyKg / (x.VirginQtyKg + x.RegrindQtyKg)) * 100m > x.MaxAllowedPct
                    && x.ApprovedBy != null),
            })
            .ToListAsync(ct);

        return grouped.Select(g => new RegrindUsageSummaryDto(
            g.ResinProductCode,
            g.TotalVirginKg,
            g.TotalRegrindKg,
            g.TotalBlendedKg,
            g.TotalBlendedKg > 0 ? (g.TotalRegrindKg / g.TotalBlendedKg) * 100m : 0m,
            g.TotalBlends,
            g.NonCompliantBlends,
            g.ApprovedNonCompliantBlends))
        .ToList();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
