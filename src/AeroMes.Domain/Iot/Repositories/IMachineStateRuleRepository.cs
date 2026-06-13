namespace AeroMes.Domain.Iot.Repositories;

public interface IMachineStateRuleRepository
{
    Task<MachineStateRule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MachineStateRule>> GetByMachineAsync(string machineCode, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(MachineStateRule entity, CancellationToken ct = default);
    void Remove(MachineStateRule entity);
}
