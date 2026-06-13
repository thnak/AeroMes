using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WarehouseZoneRepository(AppDbContext db) : IWarehouseZoneRepository
{
    public async Task<IReadOnlyList<WarehouseZone>> GetAllAsync(int? storageLocationId, bool activeOnly, CancellationToken ct)
    {
        var q = db.WarehouseZones.AsQueryable();
        if (storageLocationId.HasValue)
            q = q.Where(z => z.StorageLocationId == storageLocationId.Value);
        if (activeOnly)
            q = q.Where(z => z.IsActive);
        return await q.OrderBy(z => z.ZoneCode).AsNoTracking().ToListAsync(ct);
    }

    public Task<WarehouseZone?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WarehouseZones.AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.WarehouseZones.AnyAsync(z => z.ZoneCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(WarehouseZone entity, CancellationToken ct) =>
        await db.WarehouseZones.AddAsync(entity, ct);
}
