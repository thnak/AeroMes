using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class StorageLocationRepository(AppDbContext db) : IStorageLocationRepository
{
    public Task<StorageLocation?> GetByIdAsync(int id, CancellationToken ct) =>
        db.StorageLocations.Include(s => s.WorkCenter)
                           .FirstOrDefaultAsync(x => x.LocationID == id, ct);

    public async Task<IReadOnlyList<StorageLocation>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.StorageLocations.Include(s => s.WorkCenter).AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.LocationCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.StorageLocations.AnyAsync(x => x.LocationID == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.StorageLocations.AnyAsync(x => x.LocationCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(StorageLocation entity, CancellationToken ct)
    {
        db.StorageLocations.Add(entity);
        return Task.CompletedTask;
    }
}
