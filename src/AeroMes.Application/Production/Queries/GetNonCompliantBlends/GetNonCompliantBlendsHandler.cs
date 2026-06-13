using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetNonCompliantBlends;

public class GetNonCompliantBlendsHandler(IMaterialBlendLogRepository repo)
    : IQueryHandler<GetNonCompliantBlendsQuery, PagedResult<MaterialBlendLogDto>>
{
    public async Task<PagedResult<MaterialBlendLogDto>> HandleAsync(
        GetNonCompliantBlendsQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetNonCompliantAsync(
            query.FromDate, query.ToDate, query.IsApproved, query.Page, query.PageSize, ct);
        return new PagedResult<MaterialBlendLogDto>(items, total, query.Page, query.PageSize);
    }
}
