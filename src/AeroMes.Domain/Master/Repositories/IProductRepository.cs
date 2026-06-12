namespace AeroMes.Domain.Master.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Product?> GetByCodeWithConversionsAsync(string code, CancellationToken ct = default);
    Task<Product?> GetByCodeWithSpecificationsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetVariantsAsync(string parentCode, CancellationToken ct = default);
    Task<bool> AnyVariantLinksAsync(CancellationToken ct = default);
    Task<bool> AnySpecificationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task<bool> IsActiveAsync(string code, CancellationToken ct = default);
    Task<bool> IsReferencedAsync(string code, CancellationToken ct = default);
    Task AddAsync(Product entity, CancellationToken ct = default);
}
