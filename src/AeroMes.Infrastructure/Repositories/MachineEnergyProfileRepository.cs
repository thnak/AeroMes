using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineEnergyProfileRepository(AppDbContext db) : IMachineEnergyProfileRepository
{
    public async Task<int> AddAsync(MachineEnergyProfile profile, CancellationToken ct)
    {
        db.MachineEnergyProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return profile.ProfileID;
    }

    public Task<MachineEnergyProfile?> GetActiveByMachineAsync(string machineCode, CancellationToken ct)
        => db.MachineEnergyProfiles
            .Where(p => p.MachineCode == machineCode && p.EffectiveTo == null)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<MachineEnergyProfileDto>> GetByMachineAsync(
        string machineCode, CancellationToken ct)
        => await db.MachineEnergyProfiles.AsNoTracking()
            .Where(p => p.MachineCode == machineCode)
            .Join(db.CostEnergyTariffs.AsNoTracking(),
                p => p.TariffID,
                t => t.TariffID,
                (p, t) => new MachineEnergyProfileDto(
                    p.ProfileID, p.MachineCode,
                    p.NominalKW, p.LoadFactor,
                    p.TariffID, t.TariffName,
                    p.EffectiveFrom, p.EffectiveTo))
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
