using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Queries.GetOperatorCertifications;

public class GetOperatorCertificationsHandler(IOperatorCertificationRepository repo)
    : IQueryHandler<GetOperatorCertificationsQuery, IReadOnlyList<OperatorCertificationDto>>
{
    public async Task<IReadOnlyList<OperatorCertificationDto>> HandleAsync(GetOperatorCertificationsQuery q, CancellationToken ct)
    {
        var items = await repo.GetByEmployeeAsync(q.EmployeeCode, ct);
        return items.Select(x => new OperatorCertificationDto(
            x.CertId, x.EmployeeCode, x.CertificationCode,
            x.IssuedDate, x.ExpiryDate, x.IssuedBy, x.IsActive)).ToList();
    }
}
