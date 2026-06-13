using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class RackRepository(AppDbContext db) : IRackRepository
{
    public async Task<IReadOnlyList<Rack>> GetByAisleAsync(int aisleId, CancellationToken ct) =>
        await db.Racks.AsNoTracking()
            .Where(r => r.AisleId == aisleId)
            .OrderBy(r => r.RackCode)
            .ToListAsync(ct);

    public Task<Rack?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Racks.AsNoTracking().FirstOrDefaultAsync(r => r.RackId == id, ct);

    public Task<bool> CodeExistsInAisleAsync(int aisleId, string code, CancellationToken ct) =>
        db.Racks.AnyAsync(r => r.AisleId == aisleId && r.RackCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(Rack entity, CancellationToken ct) =>
        await db.Racks.AddAsync(entity, ct);

    public void Remove(Rack entity) => db.Racks.Remove(entity);
}
