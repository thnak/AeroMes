using AeroMes.Domain.Equipment;
using AeroMes.Domain.Equipment.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DowntimeLogRepository(AppDbContext db) : IDowntimeLogRepository
{
    public async Task<DowntimeLog?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.DowntimeLogs.FirstOrDefaultAsync(x => x.DowntimeLogID == id, ct);

    public async Task AddAsync(DowntimeLog log, CancellationToken ct = default)
        => await db.DowntimeLogs.AddAsync(log, ct);

    public async Task<double> GetTotalDowntimeMinutesAsync(
        int workCenterId,
        string machineCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var entries = await db.DowntimeLogs
            .Where(x => x.WorkCenterID == workCenterId
                && x.MachineCode == machineCode
                && x.StartTime >= from
                && x.EndTime != null
                && x.EndTime <= to)
            .Select(x => new { x.StartTime, EndTime = x.EndTime!.Value })
            .AsNoTracking()
            .ToListAsync(ct);

        return entries.Sum(x => (x.EndTime - x.StartTime).TotalMinutes);
    }
}
