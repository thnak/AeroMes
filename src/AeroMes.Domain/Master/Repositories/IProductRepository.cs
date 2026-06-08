namespace AeroMes.Domain.Master.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(Product entity, CancellationToken ct = default);
}
