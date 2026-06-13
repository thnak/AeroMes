using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CartonRepository(AppDbContext db) : ICartonRepository
{
    public async Task<IReadOnlyList<Carton>> GetByShipmentIdAsync(int shipmentId, CancellationToken ct) =>
        await db.Cartons
            .Include(c => c.Contents)
            .AsNoTracking()
            .Where(c => c.ShipmentId == shipmentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<Carton?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Cartons.AsNoTracking().FirstOrDefaultAsync(c => c.CartonId == id, ct);

    public Task<Carton?> GetByIdWithContentsAsync(int id, CancellationToken ct) =>
        db.Cartons
            .Include(c => c.Contents)
            .FirstOrDefaultAsync(c => c.CartonId == id, ct);

    public async Task AddAsync(Carton entity, CancellationToken ct) =>
        await db.Cartons.AddAsync(entity, ct);
}
