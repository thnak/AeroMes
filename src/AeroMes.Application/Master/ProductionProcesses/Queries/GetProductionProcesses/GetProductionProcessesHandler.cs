using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcesses;

public class GetProductionProcessesHandler(IProductionProcessRepository repository)
    : IQueryHandler<GetProductionProcessesQuery, PagedResult<ProductionProcessListDto>>
{
    public async Task<PagedResult<ProductionProcessListDto>> HandleAsync(
        GetProductionProcessesQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Keyword, query.ProcessType, query.IsActive, query.Page, query.PageSize, ct);
        return new PagedResult<ProductionProcessListDto>(items, total, query.Page, query.PageSize);
    }
}
