namespace AeroMes.Domain.Master.Repositories;

public interface IWorkCenterRepository
{
    Task<WorkCenter?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<WorkCenter>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(WorkCenter entity, CancellationToken ct = default);
}
