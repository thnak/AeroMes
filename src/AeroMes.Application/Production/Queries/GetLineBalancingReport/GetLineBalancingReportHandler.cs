using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetLineBalancingReport;

public class GetLineBalancingReportHandler(IBundleRepository repo)
    : IQueryHandler<GetLineBalancingReportQuery, IReadOnlyList<LineBalancingDto>>
{
    public Task<IReadOnlyList<LineBalancingDto>> HandleAsync(GetLineBalancingReportQuery query, CancellationToken ct)
        => repo.GetLineBalancingAsync(query.WorkCenterID, query.StyleCode, ct);
}
