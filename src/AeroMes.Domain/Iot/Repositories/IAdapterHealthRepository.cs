namespace AeroMes.Domain.Iot.Repositories;

public interface IAdapterHealthRepository
{
    Task<AdapterHealth?> GetByAdapterIdAsync(int adapterId, CancellationToken ct);
    Task<IReadOnlyList<AdapterHealth>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<AdapterHealthLog>> GetRecentLogsAsync(int adapterId, int limit, CancellationToken ct);
    void Add(AdapterHealth health);
    void AddLog(AdapterHealthLog log);
}
