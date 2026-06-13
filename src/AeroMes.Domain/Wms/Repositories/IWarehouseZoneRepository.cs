namespace AeroMes.Domain.Wms.Repositories;

public interface IWarehouseZoneRepository
{
    Task<IReadOnlyList<WarehouseZone>> GetAllAsync(int? storageLocationId, bool activeOnly, CancellationToken ct = default);
    Task<WarehouseZone?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(WarehouseZone entity, CancellationToken ct = default);
}
