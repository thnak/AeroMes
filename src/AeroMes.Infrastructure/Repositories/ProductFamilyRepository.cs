using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public sealed class ProductFamilyRepository(AppDbContext db) : IProductFamilyRepository
{
    public async Task<ProductFamily?> GetByCodeAsync(string familyCode, CancellationToken ct)
        => await db.ProductFamilies.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FamilyCode == familyCode, ct);

    public async Task<ProductFamily?> GetWithDimensionsAsync(string familyCode, CancellationToken ct)
        => await db.ProductFamilies
            .Include(f => f.Dimensions).ThenInclude(d => d.Values)
            .Include(f => f.Variants)
            .FirstOrDefaultAsync(f => f.FamilyCode == familyCode, ct);

    public async Task<IReadOnlyList<ProductFamily>> GetAllAsync(string? industry, bool? isActive, CancellationToken ct)
    {
        var q = db.ProductFamilies
            .Include(f => f.Dimensions)
            .Include(f => f.Variants)
            .AsNoTracking()
            .AsQueryable();

        if (industry is not null)
            q = q.Where(f => f.Industry == industry);
        if (isActive.HasValue)
            q = q.Where(f => f.IsActive == isActive.Value);

        return await q.ToListAsync(ct);
    }

    public async Task<ProductVariant?> GetVariantByKeyAsync(string familyCode, string variantKey, CancellationToken ct)
        => await db.ProductVariants.AsNoTracking()
            .FirstOrDefaultAsync(v => v.FamilyCode == familyCode && v.VariantKey == variantKey, ct);

    public async Task AddAsync(ProductFamily family, CancellationToken ct)
        => await db.ProductFamilies.AddAsync(family, ct);

    public async Task AddVariantAsync(ProductVariant variant, CancellationToken ct)
        => await db.ProductVariants.AddAsync(variant, ct);

    public async Task<bool> ExistsAsync(string familyCode, CancellationToken ct)
        => await db.ProductFamilies.AnyAsync(f => f.FamilyCode == familyCode, ct);

    public async Task<IReadOnlyList<ProductVariant>> GetVariantsAsync(string familyCode, CancellationToken ct)
        => await db.ProductVariants.AsNoTracking()
            .Where(v => v.FamilyCode == familyCode)
            .ToListAsync(ct);
}
