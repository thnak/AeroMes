namespace AeroMes.Domain.Iot.Repositories;

public interface ISignalTagRepository
{
    Task<SignalTag?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<SignalTag>> GetListAsync(string? category, string? dataType, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    Task<bool> IsInUseAsync(string key, CancellationToken ct = default);
    void Add(SignalTag tag);
    void Remove(SignalTag tag);
}
