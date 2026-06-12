using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ToolRepository(AppDbContext db) : IToolRepository
{
    public Task<Tool?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Tools
            .Include(x => x.OperationMappings)
            .Include(x => x.Checkouts.Where(c => c.ReturnedAt == null))
            .Include(x => x.MaintenanceLogs)
            .FirstOrDefaultAsync(x => x.ToolCode == code.ToUpperInvariant(), ct);

    public Task<Tool?> GetByCodeWithDetailsAsync(string code, CancellationToken ct) =>
        db.Tools.AsNoTracking()
            .Include(x => x.CurrentWorkCenter)
            .Include(x => x.OperationMappings)
            .ThenInclude(m => m.Operation)
            .Include(x => x.OperationMappings)
            .ThenInclude(m => m.Product)
            .Include(x => x.Checkouts.OrderByDescending(c => c.CheckedOutAt))
            .ThenInclude(c => c.WorkCenter)
            .Include(x => x.MaintenanceLogs)
            .FirstOrDefaultAsync(x => x.ToolCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Tools.AnyAsync(x => x.ToolCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Tool>> GetAllAsync(
        bool activeOnly = true,
        ToolType? toolType = null,
        ToolStatus? status = null,
        int? workCenterId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var q = db.Tools.AsNoTracking().AsQueryable();

        if (activeOnly) q = q.Where(x => x.IsActive);
        if (toolType is not null) q = q.Where(x => x.ToolType == toolType);
        if (status is not null) q = q.Where(x => x.Status == status);
        if (workCenterId is not null) q = q.Where(x => x.CurrentWorkCenterId == workCenterId);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.ToolCode.Contains(search) || x.ToolName.Contains(search));

        return await q.OrderBy(x => x.ToolCode).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Tool>> GetDueForCalibrationAsync(
        int withinDays = 7, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(withinDays);
        return await db.Tools.AsNoTracking()
            .Where(x => x.IsActive
                        && x.Status != ToolStatus.Scrapped
                        && x.RequiresCalibration
                        && x.NextCalibrationDue != null
                        && x.NextCalibrationDue <= cutoff)
            .OrderBy(x => x.NextCalibrationDue)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Tool>> GetDueForReconditioningAsync(
        double threshold = 0.9, CancellationToken ct = default) =>
        await db.Tools.AsNoTracking()
            .Where(x => x.IsActive
                        && x.Status != ToolStatus.Scrapped
                        && x.PmIntervalCount != null
                        && x.CurrentUsageCount - x.UsageCountAtLastPm >= x.PmIntervalCount * threshold)
            .OrderByDescending(x =>
                (double)(x.CurrentUsageCount - x.UsageCountAtLastPm) / x.PmIntervalCount!.Value)
            .ToListAsync(ct);

    public Task AddAsync(Tool entity, CancellationToken ct)
    {
        db.Tools.Add(entity);
        return Task.CompletedTask;
    }
}
