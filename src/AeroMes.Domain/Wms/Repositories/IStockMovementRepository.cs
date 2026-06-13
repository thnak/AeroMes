namespace AeroMes.Domain.Wms.Repositories;

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<StockMovement> entities, CancellationToken ct = default);
}
