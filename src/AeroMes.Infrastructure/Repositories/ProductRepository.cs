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

    public Task<Product?> GetByCodeWithConversionsAsync(string code, CancellationToken ct) =>
        db.Products
            .Include(x => x.UoMConversions)
            .FirstOrDefaultAsync(x => x.ProductCode == code.ToUpperInvariant(), ct);

    public Task<bool> IsActiveAsync(string code, CancellationToken ct) =>
        db.Products.AnyAsync(x => x.ProductCode == code.ToUpperInvariant() && x.IsActive, ct);

    public async Task<bool> IsReferencedAsync(string code, CancellationToken ct)
    {
        var normalized = code.ToUpperInvariant();
        return await db.BomItems.AnyAsync(x => x.ParentProductCode == normalized || x.ChildProductCode == normalized, ct)
            || await db.Routings.AnyAsync(x => x.ProductCode == normalized, ct)
            || await db.ProductionOrders.AnyAsync(x => x.ProductCode == normalized, ct);
    }

    public Task AddAsync(Product entity, CancellationToken ct)
    {
        db.Products.Add(entity);
        return Task.CompletedTask;
    }
}
