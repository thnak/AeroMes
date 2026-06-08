using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task<Product?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Products.FirstOrDefaultAsync(x => x.ProductCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Products.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.ProductCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        db.Products.AnyAsync(x => x.ProductCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(Product entity, CancellationToken ct)
    {
        db.Products.Add(entity);
        return Task.CompletedTask;
    }
}
