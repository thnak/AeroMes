namespace AeroMes.Domain.Wms.Repositories;

public interface IAisleRepository
{
    Task<IReadOnlyList<Aisle>> GetByZoneAsync(int zoneId, CancellationToken ct = default);
    Task<Aisle?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsInZoneAsync(int zoneId, string code, CancellationToken ct = default);
    Task AddAsync(Aisle entity, CancellationToken ct = default);
    void Remove(Aisle entity);
}
