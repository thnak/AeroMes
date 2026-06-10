using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SupplierRepository(AppDbContext db) : ISupplierRepository
{
    public Task<Supplier?> GetByIdAsync(string code, CancellationToken ct) =>
        db.Suppliers.FirstOrDefaultAsync(x => x.SupplierCode == code.ToUpperInvariant(), ct);

    public Task<Supplier?> GetByIdWithAvlAsync(string code, CancellationToken ct) =>
        db.Suppliers
            .Include(x => x.AvlItems)
            .ThenInclude(a => a.Product)
            .FirstOrDefaultAsync(x => x.SupplierCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Suppliers.AnyAsync(x => x.SupplierCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Suppliers
            .Include(x => x.AvlItems)
            .AsNoTracking()
            .AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.SupplierCode).ToListAsync(ct);
    }

    public Task AddAsync(Supplier entity, CancellationToken ct)
    {
        db.Suppliers.Add(entity);
        return Task.CompletedTask;
    }
}
