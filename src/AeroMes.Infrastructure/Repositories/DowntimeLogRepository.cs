using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DowntimeLogRepository(AppDbContext db) : IDowntimeLogRepository
{
    public Task<DowntimeLog?> GetByIdAsync(long id, CancellationToken ct) =>
        db.DowntimeLogs.FirstOrDefaultAsync(x => x.DowntimeLogID == id, ct);

    public async Task<double> GetTotalDowntimeMinutesAsync(
        string machineCode, DateTime from, DateTime to, CancellationToken ct)
    {
        var logs = await db.DowntimeLogs
            .Where(d => d.MachineCode == machineCode
                        && d.StartTime >= from
                        && (d.EndTime == null || d.EndTime <= to))
            .Select(d => new
            {
                Start = d.StartTime,
                End = d.EndTime ?? to,
            })
            .ToListAsync(ct);

        return logs.Sum(l => (l.End - l.Start).TotalMinutes);
    }

    public async Task<IReadOnlyList<DowntimeLog>> GetFilteredAsync(
        string? machineCode, bool? isOpen, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.DowntimeLogs.AsNoTracking().AsQueryable();
        if (machineCode is not null) q = q.Where(x => x.MachineCode == machineCode.ToUpperInvariant());
        if (isOpen == true) q = q.Where(x => x.EndTime == null);
        else if (isOpen == false) q = q.Where(x => x.EndTime != null);
        if (from.HasValue) q = q.Where(x => x.StartTime >= from.Value);
        if (to.HasValue) q = q.Where(x => x.StartTime <= to.Value);
        return await q.OrderByDescending(x => x.StartTime).ToListAsync(ct);
    }

    public Task AddAsync(DowntimeLog entity, CancellationToken ct)
    {
        db.DowntimeLogs.Add(entity);
        return Task.CompletedTask;
    }
}
