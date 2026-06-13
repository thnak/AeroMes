namespace AeroMes.Domain.Wms.Repositories;

public interface ICartonRepository
{
    Task<IReadOnlyList<Carton>> GetByShipmentIdAsync(int shipmentId, CancellationToken ct = default);
    Task<Carton?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Carton?> GetByIdWithContentsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Carton entity, CancellationToken ct = default);
}
