using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineProductConfigRepository(AppDbContext db) : IMachineProductConfigRepository
{
    public Task<MachineProductConfig?> GetAsync(string machineCode, string productCode, CancellationToken ct) =>
        db.MachineProductConfigs.FirstOrDefaultAsync(
            x => x.MachineCode == machineCode.ToUpperInvariant() && x.ProductCode == productCode.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<MachineProductConfig>> GetByMachineAsync(string machineCode, CancellationToken ct = default) =>
        await db.MachineProductConfigs
            .Where(x => x.MachineCode == machineCode.ToUpperInvariant())
            .OrderBy(x => x.ProductCode)
            .ToListAsync(ct);

    public Task AddAsync(MachineProductConfig entity, CancellationToken ct)
    {
        db.MachineProductConfigs.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MachineProductConfig entity) =>
        db.MachineProductConfigs.Remove(entity);
}
