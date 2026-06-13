using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBins;

public class GetBinsHandler(IBinRepository repo)
    : IQueryHandler<GetBinsQuery, IReadOnlyList<BinDto>>
{
    public async Task<IReadOnlyList<BinDto>> HandleAsync(GetBinsQuery query, CancellationToken ct)
    {
        var bins = await repo.GetByRackAsync(query.RackId, query.ActiveOnly, ct);
        return bins.Select(b => new BinDto(
            b.BinId, b.RackId, b.BinCode, b.BinLevel, b.MaxQty,
            b.BinType.ToString(), b.IsActive)).ToList();
    }
}
