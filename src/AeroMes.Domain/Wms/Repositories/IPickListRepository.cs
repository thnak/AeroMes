namespace AeroMes.Domain.Wms.Repositories;

public interface IPickListRepository
{
    Task<PickList?> GetByShipmentIdAsync(int shipmentId, CancellationToken ct = default);
    Task<PickList?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PickList?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task AddAsync(PickList entity, CancellationToken ct = default);
}
