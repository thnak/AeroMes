using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AdapterHealthRepository(AppDbContext db) : IAdapterHealthRepository
{
    public Task<AdapterHealth?> GetByAdapterIdAsync(int adapterId, CancellationToken ct) =>
        db.AdapterHealths.FirstOrDefaultAsync(h => h.AdapterId == adapterId, ct);

    public async Task<IReadOnlyList<AdapterHealth>> GetAllAsync(CancellationToken ct) =>
        await db.AdapterHealths.AsNoTracking().OrderBy(h => h.MachineCode).ToListAsync(ct);

    public async Task<IReadOnlyList<AdapterHealthLog>> GetRecentLogsAsync(int adapterId, int limit, CancellationToken ct) =>
        await db.AdapterHealthLogs
            .AsNoTracking()
            .Where(l => l.AdapterId == adapterId)
            .OrderByDescending(l => l.EventAt)
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(ct);

    public void Add(AdapterHealth health) => db.AdapterHealths.Add(health);
    public void AddLog(AdapterHealthLog log) => db.AdapterHealthLogs.Add(log);
}
