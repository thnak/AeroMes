namespace AeroMes.Domain.Master.Repositories;

public interface IMachineProductParamRepository
{
    Task<MachineProductParam?> GetAsync(string machineCode, string productCode, string paramName, CancellationToken ct = default);
    Task<IReadOnlyList<MachineProductParam>> GetByMachineAndProductAsync(string machineCode, string productCode, CancellationToken ct = default);
    Task<IReadOnlyList<MachineProductParam>> GetByMachineAsync(string machineCode, CancellationToken ct = default);
    Task AddAsync(MachineProductParam entity, CancellationToken ct = default);
    void Remove(MachineProductParam entity);
}
