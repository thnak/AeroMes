namespace AeroMes.Domain.Master.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(string code, CancellationToken ct = default);
    Task<Employee?> GetByIdWithDetailsAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<bool> IsActiveAsync(string code, CancellationToken ct = default);
    Task<bool> IsCertifiedAsync(string code, string operationCode, DateOnly asOf, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(Employee entity, CancellationToken ct = default);
}
