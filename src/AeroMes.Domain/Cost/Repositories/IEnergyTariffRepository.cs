namespace AeroMes.Domain.Cost.Repositories;

public record EnergyTariffDto(
    int TariffID, string TariffName, string TariffType,
    decimal PeakRateKWh, decimal? OffPeakRateKWh,
    TimeOnly? PeakHourStart, TimeOnly? PeakHourEnd,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive);

public interface IEnergyTariffRepository
{
    Task<int> AddAsync(EnergyTariff tariff, CancellationToken ct);
    Task<EnergyTariff?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnergyTariffDto>> GetListAsync(bool includeInactive, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
