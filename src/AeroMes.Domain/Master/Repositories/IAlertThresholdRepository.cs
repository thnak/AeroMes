namespace AeroMes.Domain.Master.Repositories;

public interface IAlertThresholdRepository
{
    Task<AlertThreshold?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AlertThreshold>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(AlertThreshold entity, CancellationToken ct = default);
}
