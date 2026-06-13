using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineStateRuleRepository(AppDbContext db) : IMachineStateRuleRepository
{
    public Task<MachineStateRule?> GetByIdAsync(int id, CancellationToken ct) =>
        db.MachineStateRules.FirstOrDefaultAsync(x => x.RuleID == id, ct);

    public async Task<IReadOnlyList<MachineStateRule>> GetByMachineAsync(string machineCode, CancellationToken ct) =>
        await db.MachineStateRules
            .Where(x => x.MachineCode == machineCode)
            .OrderBy(x => x.Priority)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.MachineStateRules.AnyAsync(x => x.RuleID == id, ct);

    public Task AddAsync(MachineStateRule entity, CancellationToken ct)
    {
        db.MachineStateRules.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MachineStateRule entity) => db.MachineStateRules.Remove(entity);
}
