namespace AeroMes.Domain.Cost.Repositories;

public record MachineEnergyProfileDto(
    int ProfileID, string MachineCode,
    decimal NominalKW, decimal LoadFactor,
    int TariffID, string TariffName,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo);

public interface IMachineEnergyProfileRepository
{
    Task<int> AddAsync(MachineEnergyProfile profile, CancellationToken ct);
    Task<MachineEnergyProfile?> GetActiveByMachineAsync(string machineCode, CancellationToken ct);
    Task<IReadOnlyList<MachineEnergyProfileDto>> GetByMachineAsync(string machineCode, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
