using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetIncompleteOrders;

public record GetIncompleteOrdersQuery(DateTime? From, DateTime? To) : IQuery<IncompleteOrdersResult>;

public class GetIncompleteOrdersQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetIncompleteOrdersQuery, IncompleteOrdersResult>
{
    public Task<IncompleteOrdersResult> HandleAsync(
        GetIncompleteOrdersQuery query, CancellationToken ct = default)
        => repo.GetIncompleteOrdersAsync(query.From, query.To, ct);
}
