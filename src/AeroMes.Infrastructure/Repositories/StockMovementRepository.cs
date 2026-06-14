using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class StockMovementRepository(AppDbContext db) : IStockMovementRepository
{
    public async Task AddAsync(StockMovement entity, CancellationToken ct) =>
        await db.StockMovements.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<StockMovement> entities, CancellationToken ct) =>
        await db.StockMovements.AddRangeAsync(entities, ct);

    public async Task<IReadOnlyList<StockMovementDto>> GetListAsync(
        string? productCode, string? lotNumber, int page, int pageSize, CancellationToken ct)
    {
        var q = db.StockMovements.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(productCode))
            q = q.Where(m => m.ProductCode == productCode.ToUpperInvariant());
        if (!string.IsNullOrEmpty(lotNumber))
            q = q.Where(m => m.LotNumber == lotNumber);

        return await q.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new StockMovementDto(
                m.MovementId, m.MovementType.ToString(), m.ProductCode,
                m.LotNumber, m.Quantity, m.LocationId,
                m.Reference, m.Notes, m.CreatedBy, m.CreatedAt))
            .ToListAsync(ct);
    }
}
