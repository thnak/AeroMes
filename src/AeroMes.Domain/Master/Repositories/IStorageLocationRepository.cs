namespace AeroMes.Domain.Master.Repositories;

public interface IStorageLocationRepository
{
    Task<StorageLocation?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<StorageLocation>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(StorageLocation entity, CancellationToken ct = default);
}
