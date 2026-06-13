using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class JobRepository(AppDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(long id, CancellationToken ct) =>
        db.Jobs.FirstOrDefaultAsync(x => x.JobID == id, ct);

    public Task<Job?> GetActiveJobAsync(int woId, string machineCode, CancellationToken ct) =>
        db.Jobs.FirstOrDefaultAsync(
            x => x.WOID == woId && x.MachineCode == machineCode && x.Status == JobStatus.Active,
            ct);

    public async Task<IReadOnlyList<Job>> GetByWoIdAsync(int woId, CancellationToken ct) =>
        await db.Jobs.AsNoTracking()
            .Where(x => x.WOID == woId)
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Job>> GetFilteredAsync(
        int? woId, string? machineCode, JobStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.Jobs.AsNoTracking().AsQueryable();
        if (woId.HasValue) q = q.Where(x => x.WOID == woId.Value);
        if (machineCode is not null) q = q.Where(x => x.MachineCode == machineCode.ToUpperInvariant());
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (from.HasValue) q = q.Where(x => x.StartTime >= from.Value);
        if (to.HasValue) q = q.Where(x => x.StartTime <= to.Value);
        return await q.OrderByDescending(x => x.StartTime).ToListAsync(ct);
    }

    public Task AddAsync(Job entity, CancellationToken ct)
    {
        db.Jobs.Add(entity);
        return Task.CompletedTask;
    }

    public Task<Job?> GetLatestCompletedJobAsync(int workOrderId, CancellationToken ct) =>
        db.Jobs
            .Where(x => x.WOID == workOrderId && x.Status == JobStatus.Finished)
            .OrderByDescending(x => x.EndTime)
            .FirstOrDefaultAsync(ct);
}
