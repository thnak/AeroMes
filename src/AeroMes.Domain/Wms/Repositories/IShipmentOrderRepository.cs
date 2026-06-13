namespace AeroMes.Domain.Wms.Repositories;

public interface IShipmentOrderRepository
{
    Task<IReadOnlyList<ShipmentOrder>> GetAllAsync(ShipmentStatus? status, CancellationToken ct = default);
    Task<ShipmentOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ShipmentOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(ShipmentOrder entity, CancellationToken ct = default);
}
