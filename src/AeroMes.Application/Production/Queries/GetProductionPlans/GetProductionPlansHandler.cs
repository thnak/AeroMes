using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionPlans;

public class GetProductionPlansHandler(IProductionPlanByOrderRepository repo)
    : IQueryHandler<GetProductionPlansQuery, PagedResult<ProductionPlanDto>>
{
    public async Task<PagedResult<ProductionPlanDto>> HandleAsync(
        GetProductionPlansQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetListAsync(
            query.PoId, query.Status,
            query.From, query.To,
            query.Page, query.PageSize, ct);
        return new PagedResult<ProductionPlanDto>(items, total, query.Page, query.PageSize);
    }
}
