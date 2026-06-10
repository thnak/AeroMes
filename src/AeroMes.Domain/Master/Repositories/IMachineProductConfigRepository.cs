namespace AeroMes.Domain.Master.Repositories;

public interface IMachineProductConfigRepository
{
    Task<MachineProductConfig?> GetAsync(string machineCode, string productCode, CancellationToken ct = default);
    Task<IReadOnlyList<MachineProductConfig>> GetByMachineAsync(string machineCode, CancellationToken ct = default);
    Task AddAsync(MachineProductConfig entity, CancellationToken ct = default);
    void Remove(MachineProductConfig entity);
}
