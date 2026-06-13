using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetDisassemblyOrders;

public class GetDisassemblyOrdersHandler(IDisassemblyOrderRepository repo)
    : IQueryHandler<GetDisassemblyOrdersQuery, PagedResult<DisassemblyOrderDto>>
{
    public async Task<PagedResult<DisassemblyOrderDto>> HandleAsync(
        GetDisassemblyOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetListAsync(
            query.SourceProductCode, query.Status, query.FromDate, query.ToDate,
            query.Page, query.PageSize, ct);
        return new PagedResult<DisassemblyOrderDto>(items, total, query.Page, query.PageSize);
    }
}
