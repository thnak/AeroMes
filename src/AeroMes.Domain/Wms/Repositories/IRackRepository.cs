namespace AeroMes.Domain.Wms.Repositories;

public interface IRackRepository
{
    Task<IReadOnlyList<Rack>> GetByAisleAsync(int aisleId, CancellationToken ct = default);
    Task<Rack?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsInAisleAsync(int aisleId, string code, CancellationToken ct = default);
    Task AddAsync(Rack entity, CancellationToken ct = default);
    void Remove(Rack entity);
}
