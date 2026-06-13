using AeroMes.Application.Common;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetReworkOrders;

public class GetReworkOrdersHandler(IReworkOrderRepository repository)
    : IQueryHandler<GetReworkOrdersQuery, PagedResult<ReworkOrderDto>>
{
    public async Task<PagedResult<ReworkOrderDto>> HandleAsync(GetReworkOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(query.Status, query.ProductCode, query.Page, query.PageSize, ct);
        return new PagedResult<ReworkOrderDto>(items, total, query.Page, query.PageSize);
    }
}
