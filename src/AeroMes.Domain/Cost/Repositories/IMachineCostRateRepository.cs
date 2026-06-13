namespace AeroMes.Domain.Cost.Repositories;

public record MachineCostRateDto(
    int RateID, string MachineCode, string RateType,
    decimal RatePerHour, DateOnly EffectiveFrom, DateOnly? EffectiveTo,
    string? Notes, DateTime CreatedAt);

public record MachineTotalRateDto(string MachineCode, decimal TotalRatePerHour, int ActiveRateCount);

public interface IMachineCostRateRepository
{
    Task<int> AddAsync(MachineCostRate rate, CancellationToken ct);
    Task<MachineCostRate?> GetActiveAsync(string machineCode, MachineCostRateType rateType, CancellationToken ct);
    Task<IReadOnlyList<MachineCostRateDto>> GetByMachineAsync(string machineCode, bool includeExpired, CancellationToken ct);
    Task<MachineTotalRateDto> GetTotalRateAsync(string machineCode, CancellationToken ct);
    Task<bool> HasOverlapAsync(string machineCode, MachineCostRateType rateType, DateOnly from, DateOnly? to, int? excludeId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
