using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AdapterRepository(AppDbContext db) : IAdapterRepository
{
    public Task<AdapterInstance?> GetByIdAsync(int id, CancellationToken ct) =>
        db.AdapterInstances.FirstOrDefaultAsync(x => x.AdapterID == id, ct);

    public Task<AdapterInstance?> GetByIdWithSignalsAsync(int id, CancellationToken ct) =>
        db.AdapterInstances.Include(x => x.Signals).FirstOrDefaultAsync(x => x.AdapterID == id, ct);

    public async Task<IReadOnlyList<AdapterInstance>> GetByMachineAsync(string machineCode, CancellationToken ct) =>
        await db.AdapterInstances
            .Include(x => x.Signals)
            .Where(x => x.MachineCode == machineCode)
            .OrderBy(x => x.AdapterID)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AdapterInstance>> GetEnabledByTypeAsync(AdapterType type, CancellationToken ct) =>
        await db.AdapterInstances
            .Where(x => x.IsEnabled && x.AdapterType == type)
            .OrderBy(x => x.AdapterID)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.AdapterInstances.AnyAsync(x => x.AdapterID == id, ct);

    public Task AddAsync(AdapterInstance entity, CancellationToken ct)
    {
        db.AdapterInstances.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(AdapterInstance entity) => db.AdapterInstances.Remove(entity);
}
