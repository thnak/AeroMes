namespace AeroMes.Domain.Master.Repositories;

public interface IProductAttributeRepository
{
    Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductAttribute?> GetByIdWithValuesAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ProductAttribute>> GetAllAsync(bool activeOnly = true, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetValueGroupNamesAsync(CancellationToken ct = default);
    Task AddAsync(ProductAttribute entity, CancellationToken ct = default);

    Task<bool> HasAssignmentsAsync(int attributeId, CancellationToken ct = default);
    Task<bool> IsValueInUseAsync(int valueId, CancellationToken ct = default);
    Task<ProductAttributeAssignment?> GetAssignmentAsync(string productCode, int attributeId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductAttributeAssignment>> GetAssignmentsForProductAsync(string productCode, CancellationToken ct = default);
    Task AddAssignmentAsync(ProductAttributeAssignment entity, CancellationToken ct = default);
}
