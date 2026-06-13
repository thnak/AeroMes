using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AisleRepository(AppDbContext db) : IAisleRepository
{
    public async Task<IReadOnlyList<Aisle>> GetByZoneAsync(int zoneId, CancellationToken ct) =>
        await db.Aisles.AsNoTracking()
            .Where(a => a.ZoneId == zoneId)
            .OrderBy(a => a.PickSequence).ThenBy(a => a.AisleCode)
            .ToListAsync(ct);

    public Task<Aisle?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Aisles.AsNoTracking().FirstOrDefaultAsync(a => a.AisleId == id, ct);

    public Task<bool> CodeExistsInZoneAsync(int zoneId, string code, CancellationToken ct) =>
        db.Aisles.AnyAsync(a => a.ZoneId == zoneId && a.AisleCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(Aisle entity, CancellationToken ct) =>
        await db.Aisles.AddAsync(entity, ct);

    public void Remove(Aisle entity) => db.Aisles.Remove(entity);
}
