using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductCategoryRepository(AppDbContext db) : IProductCategoryRepository
{
    public Task<ProductCategory?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductCategories.FirstOrDefaultAsync(x => x.CategoryId == id, ct);

    public Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.ProductCategories.FirstOrDefaultAsync(x => x.CategoryCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.ProductCategories.AnyAsync(x => x.CategoryCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<ProductCategory>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.ProductCategories.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.CategoryCode).ToListAsync(ct);
    }

    public Task<bool> IsActiveAsync(int categoryId, CancellationToken ct) =>
        db.ProductCategories.AnyAsync(x => x.CategoryId == categoryId && x.IsActive, ct);

    public Task<bool> HasProductsAsync(int categoryId, CancellationToken ct) =>
        db.Products.AnyAsync(x => x.CategoryId == categoryId, ct);

    public Task<bool> HasChildrenAsync(int categoryId, CancellationToken ct) =>
        db.ProductCategories.AnyAsync(x => x.ParentId == categoryId, ct);

    public Task AddAsync(ProductCategory entity, CancellationToken ct)
    {
        db.ProductCategories.Add(entity);
        return Task.CompletedTask;
    }
}
