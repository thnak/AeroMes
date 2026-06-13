using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrdersList;

public class GetSalesOrdersListHandler(
    ISalesOrderRepository repo) : IQueryHandler<GetSalesOrdersListQuery, IReadOnlyList<SalesOrderSummaryDto>>
{
    public Task<IReadOnlyList<SalesOrderSummaryDto>> HandleAsync(
        GetSalesOrdersListQuery query, CancellationToken ct = default)
        => repo.GetListAsync(query.SoCode, query.Status, query.IncludeUnconfirmed, query.From, query.To, ct);
}
