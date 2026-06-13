using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetOperatorEfficiencyReport;

public class GetOperatorEfficiencyReportHandler(IBundleRepository repo)
    : IQueryHandler<GetOperatorEfficiencyReportQuery, IReadOnlyList<OperatorEfficiencyDto>>
{
    public Task<IReadOnlyList<OperatorEfficiencyDto>> HandleAsync(
        GetOperatorEfficiencyReportQuery query, CancellationToken ct)
        => repo.GetOperatorEfficiencyAsync(query.OperatorID, query.FromDate, query.ToDate, ct);
}
