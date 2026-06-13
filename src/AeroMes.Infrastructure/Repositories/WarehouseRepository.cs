using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WarehouseRepository(AppDbContext db) : IWarehouseRepository
{
    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(bool activeOnly, string? search, CancellationToken ct)
    {
        var q = db.Warehouses.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            q = q.Where(x => x.WarehouseCode.Contains(term)
                           || x.WarehouseName.Contains(search.Trim())
                           || (x.Address != null && x.Address.Contains(search.Trim())));
        }
        return await q.OrderBy(x => x.WarehouseCode).AsNoTracking().ToListAsync(ct);
    }

    public Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == id, ct);

    public Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Warehouses.FirstOrDefaultAsync(x => x.WarehouseCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Warehouses.AnyAsync(x => x.WarehouseCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(Warehouse entity, CancellationToken ct)
    {
        db.Warehouses.Add(entity);
        return Task.CompletedTask;
    }
}
