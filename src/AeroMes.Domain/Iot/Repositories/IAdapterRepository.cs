namespace AeroMes.Domain.Iot.Repositories;

public interface IAdapterRepository
{
    Task<AdapterInstance?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AdapterInstance?> GetByIdWithSignalsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AdapterInstance>> GetByMachineAsync(string machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<AdapterInstance>> GetEnabledByTypeAsync(AdapterType type, CancellationToken ct = default);
    Task<AdapterInstance?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(AdapterInstance entity, CancellationToken ct = default);
    void Remove(AdapterInstance entity);
}
