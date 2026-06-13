using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineCostRateRepository(AppDbContext db) : IMachineCostRateRepository
{
    public async Task<int> AddAsync(MachineCostRate rate, CancellationToken ct)
    {
        db.MachineCostRates.Add(rate);
        await db.SaveChangesAsync(ct);
        return rate.RateID;
    }

    public Task<MachineCostRate?> GetActiveAsync(string machineCode, MachineCostRateType rateType, CancellationToken ct)
        => db.MachineCostRates
            .Where(r => r.MachineCode == machineCode && r.RateType == rateType && r.EffectiveTo == null)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<MachineCostRateDto>> GetByMachineAsync(
        string machineCode, bool includeExpired, CancellationToken ct)
    {
        var q = db.MachineCostRates.AsNoTracking().Where(r => r.MachineCode == machineCode);
        if (!includeExpired) q = q.Where(r => r.EffectiveTo == null);

        return await q.OrderBy(r => r.RateType).ThenByDescending(r => r.EffectiveFrom)
            .Select(r => new MachineCostRateDto(
                r.RateID, r.MachineCode, r.RateType.ToString(),
                r.RatePerHour, r.EffectiveFrom, r.EffectiveTo, r.Notes, r.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<MachineTotalRateDto> GetTotalRateAsync(string machineCode, CancellationToken ct)
    {
        var rates = await db.MachineCostRates.AsNoTracking()
            .Where(r => r.MachineCode == machineCode && r.EffectiveTo == null)
            .ToListAsync(ct);

        return new MachineTotalRateDto(
            machineCode,
            rates.Sum(r => r.RatePerHour),
            rates.Count);
    }

    public Task<bool> HasOverlapAsync(string machineCode, MachineCostRateType rateType,
        DateOnly from, DateOnly? to, int? excludeId, CancellationToken ct)
        => db.MachineCostRates.AnyAsync(r =>
            r.MachineCode == machineCode &&
            r.RateType == rateType &&
            r.EffectiveTo == null &&
            (excludeId == null || r.RateID != excludeId), ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
