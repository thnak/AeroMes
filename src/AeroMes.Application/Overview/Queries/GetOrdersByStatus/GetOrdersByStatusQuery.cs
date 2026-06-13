using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetOrdersByStatus;

public record GetOrdersByStatusQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<OrdersByStatusItem>>;

public class GetOrdersByStatusQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetOrdersByStatusQuery, IReadOnlyList<OrdersByStatusItem>>
{
    public Task<IReadOnlyList<OrdersByStatusItem>> HandleAsync(
        GetOrdersByStatusQuery query, CancellationToken ct = default)
        => repo.GetOrdersByStatusAsync(query.From, query.To, ct);
}
