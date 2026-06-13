namespace AeroMes.Domain.Iot.Repositories;

public interface ISignalMappingRepository
{
    Task<SignalMapping?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SignalMapping>> GetByAdapterAsync(int adapterId, CancellationToken ct = default);
    Task<bool> TagKeyExistsAsync(int adapterId, string tagKey, CancellationToken ct = default);
    Task AddAsync(SignalMapping entity, CancellationToken ct = default);
    void Remove(SignalMapping entity);
}
