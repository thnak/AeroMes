namespace AeroMes.Domain.Energy.Repositories;

public record MeterDto(
    int MeterID, string MeterCode, string MeterName, string UtilityType,
    string Unit, string? MachineCode, int? WorkCenterID, bool IsSubMeter,
    int? ParentMeterID, int? TariffID, bool IsActive);

public record ShiftEnergyDto(
    string MachineCode, string ShiftCode, DateOnly ShiftDate,
    decimal? ConsumedUnits, decimal? EnergyCost, int? QtyProduced, decimal? EnergyIntensity,
    string Unit);

public record EnergyIntensityTrendDto(DateOnly ShiftDate, decimal? EnergyIntensity, decimal? TargetKWhPerUnit);

public interface IEnergyRepository
{
    Task AddMeterAsync(Meter meter, CancellationToken ct);
    Task<Meter?> GetMeterByIdAsync(int meterId, CancellationToken ct);
    Task<Meter?> GetMeterByCodeAsync(string code, CancellationToken ct);
    Task<bool> MeterCodeExistsAsync(string code, CancellationToken ct);

    Task AddReadingAsync(MeterReading reading, CancellationToken ct);
    Task<MeterReading?> GetReadingByIdAsync(long readingId, CancellationToken ct);

    Task AddConsumptionAsync(ShiftConsumption consumption, CancellationToken ct);
    Task<ShiftConsumption?> GetOpenShiftConsumptionAsync(int meterId, string shiftCode, DateOnly date, CancellationToken ct);

    Task<IReadOnlyList<ShiftEnergyDto>> GetShiftReportAsync(
        string? machineCode, DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyList<EnergyIntensityTrendDto>> GetIntensityTrendAsync(
        string machineCode, int months, CancellationToken ct);
    Task<EnergyTarget?> GetActiveTargetAsync(string machineCode, DateOnly onDate, CancellationToken ct);
}
