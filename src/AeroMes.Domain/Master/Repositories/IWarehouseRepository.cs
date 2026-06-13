namespace AeroMes.Domain.Master.Repositories;

public interface IWarehouseRepository
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync(bool activeOnly, string? search, CancellationToken ct = default);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(Warehouse entity, CancellationToken ct = default);
}
