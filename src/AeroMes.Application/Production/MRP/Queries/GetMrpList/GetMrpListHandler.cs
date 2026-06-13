using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MRP.Queries.GetMrpList;

public class GetMrpListHandler(IMaterialRequirementsPlanRepository repository)
    : IQueryHandler<GetMrpListQuery, PagedResult<MrpListDto>>
{
    public async Task<PagedResult<MrpListDto>> HandleAsync(
        GetMrpListQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Keyword, query.MasterPlanId, query.Status,
            query.Page, query.PageSize, ct);
        return new PagedResult<MrpListDto>(items, total, query.Page, query.PageSize);
    }
}
