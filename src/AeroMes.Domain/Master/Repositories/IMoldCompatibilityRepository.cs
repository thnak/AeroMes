namespace AeroMes.Domain.Master.Repositories;

public record MoldCompatibilityDto(
    string MoldCode,
    string MachineCode,
    bool IsCompatible,
    string? Notes);

public interface IMoldCompatibilityRepository
{
    Task<bool> IsCompatibleAsync(string moldCode, string machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<MoldCompatibilityDto>> GetCompatibleMoldsAsync(string machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<MoldCompatibilityDto>> GetCompatibleMachinesAsync(string moldCode, CancellationToken ct = default);
    Task UpsertAsync(MoldMachineCompatibility compatibility, CancellationToken ct = default);
    Task RemoveAsync(string moldCode, string machineCode, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
