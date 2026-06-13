namespace AeroMes.Domain.Master.Repositories;

public interface IProductFamilyRepository
{
    Task<ProductFamily?> GetByCodeAsync(string familyCode, CancellationToken ct);
    Task<ProductFamily?> GetWithDimensionsAsync(string familyCode, CancellationToken ct);
    Task<IReadOnlyList<ProductFamily>> GetAllAsync(string? industry, bool? isActive, CancellationToken ct);
    Task<ProductVariant?> GetVariantByKeyAsync(string familyCode, string variantKey, CancellationToken ct);
    Task AddAsync(ProductFamily family, CancellationToken ct);
    Task AddVariantAsync(ProductVariant variant, CancellationToken ct);
    Task<bool> ExistsAsync(string familyCode, CancellationToken ct);
    Task<IReadOnlyList<ProductVariant>> GetVariantsAsync(string familyCode, CancellationToken ct);
}
