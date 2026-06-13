using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class OperatorCertificationRepository(AppDbContext db) : IOperatorCertificationRepository
{
    public async Task<IReadOnlyList<OperatorCertification>> GetByEmployeeAsync(string employeeCode, CancellationToken ct) =>
        await db.OperatorCertifications
            .Where(x => x.EmployeeCode == employeeCode.ToUpperInvariant())
            .OrderByDescending(x => x.IssuedDate)
            .ToListAsync(ct);

    public Task<OperatorCertification?> GetActiveAsync(string employeeCode, string certificationCode, CancellationToken ct) =>
        db.OperatorCertifications.FirstOrDefaultAsync(
            x => x.EmployeeCode == employeeCode.ToUpperInvariant()
              && x.CertificationCode == certificationCode.ToUpperInvariant()
              && x.IsActive, ct);

    public Task AddAsync(OperatorCertification entity, CancellationToken ct)
    {
        db.OperatorCertifications.Add(entity);
        return Task.CompletedTask;
    }
}
