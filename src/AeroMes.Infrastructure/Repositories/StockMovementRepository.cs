using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;

namespace AeroMes.Infrastructure.Repositories;

public class StockMovementRepository(AppDbContext db) : IStockMovementRepository
{
    public async Task AddAsync(StockMovement entity, CancellationToken ct) =>
        await db.StockMovements.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<StockMovement> entities, CancellationToken ct) =>
        await db.StockMovements.AddRangeAsync(entities, ct);
}
