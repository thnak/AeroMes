using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetStandardSets;

public class GetStandardSetsHandler(IQualityStandardSetRepository repository)
    : IQueryHandler<GetStandardSetsQuery, PagedResult<StandardSetListDto>>
{
    public async Task<PagedResult<StandardSetListDto>> HandleAsync(
        GetStandardSetsQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Keyword, query.ProductCode, query.Status,
            query.Page, query.PageSize, ct);
        return new PagedResult<StandardSetListDto>(items, total, query.Page, query.PageSize);
    }
}
