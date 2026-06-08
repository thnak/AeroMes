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

    public Task AddAsync(DowntimeLog entity, CancellationToken ct)
    {
        db.DowntimeLogs.Add(entity);
        return Task.CompletedTask;
    }
}
