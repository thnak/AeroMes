using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetBomVersionDetail;

public class GetBomVersionDetailHandler(IBomHeaderRepository repo)
    : IQueryHandler<GetBomVersionDetailQuery, BomVersionDetailDto?>
{
    public async Task<BomVersionDetailDto?> HandleAsync(
        GetBomVersionDetailQuery query, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionWithDetailsAsync(query.ProductCode, query.Version, ct);
        return header is null ? null : GetActiveBomHandler.ToDetailDto(header);
    }
}
