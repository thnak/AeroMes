using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum EnergyTariffType { FLAT, TIME_OF_USE }

public class EnergyTariff : Entity
{
    public int TariffID { get; private set; }
    public string TariffName { get; private set; } = string.Empty;
    public EnergyTariffType TariffType { get; private set; }
    public decimal PeakRateKWh { get; private set; }
    public decimal? OffPeakRateKWh { get; private set; }
    public TimeOnly? PeakHourStart { get; private set; }
    public TimeOnly? PeakHourEnd { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; } = true;

    private EnergyTariff() { }

    public static EnergyTariff Create(
        string tariffName, EnergyTariffType tariffType,
        decimal peakRateKWh, decimal? offPeakRateKWh,
        TimeOnly? peakHourStart, TimeOnly? peakHourEnd,
        DateOnly effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(tariffName)) throw new DomainException("Tên biểu giá không được để trống.");
        if (peakRateKWh <= 0) throw new DomainException("Giá điện giờ cao điểm phải lớn hơn 0.");
        return new EnergyTariff
        {
            TariffName = tariffName.Trim(), TariffType = tariffType,
            PeakRateKWh = peakRateKWh, OffPeakRateKWh = offPeakRateKWh,
            PeakHourStart = peakHourStart, PeakHourEnd = peakHourEnd,
            EffectiveFrom = effectiveFrom
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
