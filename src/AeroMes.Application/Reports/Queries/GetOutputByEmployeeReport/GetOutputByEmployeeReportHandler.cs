using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOutputByEmployeeReport;

public class GetOutputByEmployeeReportHandler(IProductionLogRepository repo)
    : IQueryHandler<GetOutputByEmployeeReportQuery, IReadOnlyList<EmployeeOutputDto>>
{
    public Task<IReadOnlyList<EmployeeOutputDto>> HandleAsync(
        GetOutputByEmployeeReportQuery q, CancellationToken ct = default)
        => repo.GetOutputByEmployeeAsync(q.From, q.To, ct);
}
