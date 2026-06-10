namespace AeroMes.Domain.Master.Repositories;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategory>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(ProductCategory entity, CancellationToken ct = default);
}
