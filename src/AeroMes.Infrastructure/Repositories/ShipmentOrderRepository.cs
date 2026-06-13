using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ShipmentOrderRepository(AppDbContext db) : IShipmentOrderRepository
{
    public async Task<IReadOnlyList<ShipmentOrder>> GetAllAsync(ShipmentStatus? status, CancellationToken ct)
    {
        var q = db.ShipmentOrders.Include(s => s.Lines).AsNoTracking().AsQueryable();
        if (status.HasValue)
            q = q.Where(s => s.Status == status.Value);
        return await q.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
    }

    public Task<ShipmentOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ShipmentOrders.AsNoTracking().FirstOrDefaultAsync(s => s.ShipmentId == id, ct);

    public Task<ShipmentOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.ShipmentOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ShipmentId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.ShipmentOrders.AnyAsync(s => s.ShipmentCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(ShipmentOrder entity, CancellationToken ct) =>
        await db.ShipmentOrders.AddAsync(entity, ct);
}
