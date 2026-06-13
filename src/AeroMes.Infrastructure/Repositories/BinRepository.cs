using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class BinRepository(AppDbContext db) : IBinRepository
{
    public async Task<IReadOnlyList<Bin>> GetByRackAsync(int rackId, bool activeOnly, CancellationToken ct)
    {
        var q = db.Bins.AsNoTracking().Where(b => b.RackId == rackId);
        if (activeOnly) q = q.Where(b => b.IsActive);
        return await q.OrderBy(b => b.BinLevel).ThenBy(b => b.BinCode).ToListAsync(ct);
    }

    public Task<Bin?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Bins.AsNoTracking().FirstOrDefaultAsync(b => b.BinId == id, ct);

    public Task<bool> CodeExistsInRackAsync(int rackId, string code, CancellationToken ct) =>
        db.Bins.AnyAsync(b => b.RackId == rackId && b.BinCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(Bin entity, CancellationToken ct) =>
        await db.Bins.AddAsync(entity, ct);

    public void Remove(Bin entity) => db.Bins.Remove(entity);
}
