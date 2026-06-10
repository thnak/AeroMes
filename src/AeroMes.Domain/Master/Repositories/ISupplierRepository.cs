namespace AeroMes.Domain.Master.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(string code, CancellationToken ct = default);
    Task<Supplier?> GetByIdWithAvlAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(Supplier entity, CancellationToken ct = default);
}
