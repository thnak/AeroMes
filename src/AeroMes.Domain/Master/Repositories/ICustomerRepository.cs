namespace AeroMes.Domain.Master.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string code, CancellationToken ct = default);
    Task<Customer?> GetByIdWithDetailsAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<CustomerPartNumber?> GetPartNumberAsync(string customerCode, string customerPartNo, CancellationToken ct = default);
    Task AddAsync(Customer entity, CancellationToken ct = default);
}
