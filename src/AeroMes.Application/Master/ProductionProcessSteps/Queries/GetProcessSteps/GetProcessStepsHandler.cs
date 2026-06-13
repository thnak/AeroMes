using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Queries.GetProcessSteps;

public class GetProcessStepsHandler(IProductionProcessStepRepository repository)
    : IQueryHandler<GetProcessStepsQuery, PagedResult<ProductionProcessStepDto>>
{
    public async Task<PagedResult<ProductionProcessStepDto>> HandleAsync(
        GetProcessStepsQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Keyword, query.Scope, query.IsActive, query.Page, query.PageSize, ct);
        return new PagedResult<ProductionProcessStepDto>(items, total, query.Page, query.PageSize);
    }
}
