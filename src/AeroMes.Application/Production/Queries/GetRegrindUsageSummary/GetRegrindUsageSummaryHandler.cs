using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetRegrindUsageSummary;

public class GetRegrindUsageSummaryHandler(IMaterialBlendLogRepository repo)
    : IQueryHandler<GetRegrindUsageSummaryQuery, IReadOnlyList<RegrindUsageSummaryDto>>
{
    public Task<IReadOnlyList<RegrindUsageSummaryDto>> HandleAsync(
        GetRegrindUsageSummaryQuery query, CancellationToken ct)
        => repo.GetSummaryAsync(query.ResinProductCode, query.FromDate, query.ToDate, ct);
}
