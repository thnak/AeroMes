using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetWIPByStyle;

public class GetWIPByStyleHandler(IBundleRepository repo)
    : IQueryHandler<GetWIPByStyleQuery, IReadOnlyList<WIPByStyleDto>>
{
    public Task<IReadOnlyList<WIPByStyleDto>> HandleAsync(GetWIPByStyleQuery query, CancellationToken ct)
        => repo.GetWIPByStyleAsync(query.StyleCode, query.ColorCode, query.WOID, ct);
}
