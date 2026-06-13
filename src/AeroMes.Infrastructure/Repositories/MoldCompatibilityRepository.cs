using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MoldCompatibilityRepository(AppDbContext db) : IMoldCompatibilityRepository
{
    public Task<bool> IsCompatibleAsync(string moldCode, string machineCode, CancellationToken ct)
        => db.MoldMachineCompatibilities.AnyAsync(
            c => c.MoldCode == moldCode && c.MachineCode == machineCode && c.IsCompatible, ct);

    public async Task<IReadOnlyList<MoldCompatibilityDto>> GetCompatibleMoldsAsync(
        string machineCode, CancellationToken ct)
        => await db.MoldMachineCompatibilities.AsNoTracking()
            .Where(c => c.MachineCode == machineCode && c.IsCompatible)
            .Select(c => new MoldCompatibilityDto(c.MoldCode, c.MachineCode, c.IsCompatible, c.Notes))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MoldCompatibilityDto>> GetCompatibleMachinesAsync(
        string moldCode, CancellationToken ct)
        => await db.MoldMachineCompatibilities.AsNoTracking()
            .Where(c => c.MoldCode == moldCode && c.IsCompatible)
            .Select(c => new MoldCompatibilityDto(c.MoldCode, c.MachineCode, c.IsCompatible, c.Notes))
            .ToListAsync(ct);

    public async Task UpsertAsync(MoldMachineCompatibility compat, CancellationToken ct)
    {
        var existing = await db.MoldMachineCompatibilities
            .FirstOrDefaultAsync(c => c.MoldCode == compat.MoldCode && c.MachineCode == compat.MachineCode, ct);
        if (existing is null)
            db.MoldMachineCompatibilities.Add(compat);
        else
            existing.UpdateCompatibility(compat.IsCompatible, compat.Notes);
    }

    public async Task RemoveAsync(string moldCode, string machineCode, CancellationToken ct)
    {
        var existing = await db.MoldMachineCompatibilities
            .FirstOrDefaultAsync(c => c.MoldCode == moldCode && c.MachineCode == machineCode, ct);
        if (existing != null)
            db.MoldMachineCompatibilities.Remove(existing);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
