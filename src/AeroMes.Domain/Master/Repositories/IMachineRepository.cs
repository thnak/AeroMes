namespace AeroMes.Domain.Master.Repositories;

public interface IMachineRepository
{
    Task<Machine?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Machine>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(Machine entity, CancellationToken ct = default);
}
