namespace AeroMes.Domain.Master.Repositories;

public interface IOperatorCertificationRepository
{
    Task<IReadOnlyList<OperatorCertification>> GetByEmployeeAsync(string employeeCode, CancellationToken ct = default);
    Task<OperatorCertification?> GetActiveAsync(string employeeCode, string certificationCode, CancellationToken ct = default);
    Task AddAsync(OperatorCertification entity, CancellationToken ct = default);
}
