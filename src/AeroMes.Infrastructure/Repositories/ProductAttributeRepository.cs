using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductAttributeRepository(AppDbContext db) : IProductAttributeRepository
{
    public Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductAttributes.FirstOrDefaultAsync(x => x.AttributeId == id, ct);

    public Task<ProductAttribute?> GetByIdWithValuesAsync(int id, CancellationToken ct) =>
        db.ProductAttributes
            .Include(x => x.Values)
            .FirstOrDefaultAsync(x => x.AttributeId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.ProductAttributes.AnyAsync(x => x.AttributeCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<ProductAttribute>> GetAllAsync(bool activeOnly, string? search, CancellationToken ct)
    {
        var query = db.ProductAttributes.AsNoTracking().Include(x => x.Values).AsQueryable();
        if (activeOnly)
            query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.AttributeCode.Contains(search) || x.AttributeName.Contains(search));
        return await query.OrderBy(x => x.AttributeCode).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetValueGroupNamesAsync(CancellationToken ct) =>
        await db.Set<ProductAttributeValue>()
            .AsNoTracking()
            .Where(x => x.GroupName != null)
            .Select(x => x.GroupName!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

    public Task AddAsync(ProductAttribute entity, CancellationToken ct)
    {
        db.ProductAttributes.Add(entity);
        return Task.CompletedTask;
    }

    public Task<bool> HasAssignmentsAsync(int attributeId, CancellationToken ct) =>
        db.ProductAttributeAssignments.AnyAsync(x => x.AttributeId == attributeId, ct);

    public Task<bool> IsValueInUseAsync(int valueId, CancellationToken ct) =>
        db.ProductAttributeAssignments.AnyAsync(x => x.SelectedValueId == valueId, ct);

    public Task<ProductAttributeAssignment?> GetAssignmentAsync(string productCode, int attributeId, CancellationToken ct) =>
        db.ProductAttributeAssignments
            .FirstOrDefaultAsync(x => x.ProductCode == productCode.ToUpperInvariant() && x.AttributeId == attributeId, ct);

    public async Task<IReadOnlyList<ProductAttributeAssignment>> GetAssignmentsForProductAsync(string productCode, CancellationToken ct) =>
        await db.ProductAttributeAssignments
            .AsNoTracking()
            .Include(x => x.Attribute)
            .Include(x => x.SelectedValue)
            .Where(x => x.ProductCode == productCode.ToUpperInvariant())
            .OrderBy(x => x.Attribute!.AttributeCode)
            .ToListAsync(ct);

    public Task AddAssignmentAsync(ProductAttributeAssignment entity, CancellationToken ct)
    {
        db.ProductAttributeAssignments.Add(entity);
        return Task.CompletedTask;
    }
}
