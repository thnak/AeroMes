namespace AeroMes.Domain.Wms.Repositories;

public interface IBinRepository
{
    Task<IReadOnlyList<Bin>> GetByRackAsync(int rackId, bool activeOnly, CancellationToken ct = default);
    Task<Bin?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsInRackAsync(int rackId, string code, CancellationToken ct = default);
    Task AddAsync(Bin entity, CancellationToken ct = default);
    void Remove(Bin entity);
}
