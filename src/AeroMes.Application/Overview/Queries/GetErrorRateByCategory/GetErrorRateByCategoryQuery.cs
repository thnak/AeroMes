using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetErrorRateByCategory;

public record GetErrorRateByCategoryQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<ErrorRateByCategoryItem>>;

public class GetErrorRateByCategoryQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetErrorRateByCategoryQuery, IReadOnlyList<ErrorRateByCategoryItem>>
{
    public Task<IReadOnlyList<ErrorRateByCategoryItem>> HandleAsync(
        GetErrorRateByCategoryQuery query, CancellationToken ct = default)
        => repo.GetErrorRateByCategoryAsync(query.From, query.To, ct);
}
