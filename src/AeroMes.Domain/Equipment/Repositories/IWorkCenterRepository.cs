namespace AeroMes.Domain.Equipment.Repositories;

public interface IWorkCenterRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<WorkCenter?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(WorkCenter workCenter, CancellationToken ct = default);
}
