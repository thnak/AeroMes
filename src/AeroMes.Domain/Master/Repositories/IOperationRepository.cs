namespace AeroMes.Domain.Master.Repositories;

public interface IOperationRepository
{
    Task<Operation?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Operation>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(Operation entity, CancellationToken ct = default);
}
