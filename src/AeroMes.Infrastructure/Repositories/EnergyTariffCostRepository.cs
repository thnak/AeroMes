using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class EnergyTariffCostRepository(AppDbContext db) : IEnergyTariffRepository
{
    public async Task<int> AddAsync(EnergyTariff tariff, CancellationToken ct)
    {
        db.CostEnergyTariffs.Add(tariff);
        await db.SaveChangesAsync(ct);
        return tariff.TariffID;
    }

    public Task<EnergyTariff?> GetByIdAsync(int id, CancellationToken ct)
        => db.CostEnergyTariffs.FirstOrDefaultAsync(t => t.TariffID == id, ct);

    public async Task<IReadOnlyList<EnergyTariffDto>> GetListAsync(bool includeInactive, CancellationToken ct)
    {
        var q = db.CostEnergyTariffs.AsNoTracking();
        if (!includeInactive) q = q.Where(t => t.IsActive);

        return await q.OrderByDescending(t => t.EffectiveFrom)
            .Select(t => new EnergyTariffDto(
                t.TariffID, t.TariffName, t.TariffType.ToString(),
                t.PeakRateKWh, t.OffPeakRateKWh,
                t.PeakHourStart, t.PeakHourEnd,
                t.EffectiveFrom, t.EffectiveTo, t.IsActive))
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
