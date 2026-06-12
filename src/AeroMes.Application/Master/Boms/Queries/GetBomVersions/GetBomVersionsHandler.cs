using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetBomVersions;

public class GetBomVersionsHandler(IBomHeaderRepository repo)
    : IQueryHandler<GetBomVersionsQuery, IReadOnlyList<BomVersionDto>>
{
    public async Task<IReadOnlyList<BomVersionDto>> HandleAsync(
        GetBomVersionsQuery query, CancellationToken ct)
    {
        var versions = await repo.GetVersionsByProductAsync(query.ProductCode, ct);

        return versions
            .Select(h => new BomVersionDto(
                h.BomHeaderId, h.Version, h.Status.ToString(),
                h.EffectiveFrom, h.EffectiveTo, h.BaseQuantity,
                h.EcoReference, h.ApprovedBy, h.ApprovedAt,
                h.Lines.Count, h.CreatedAt))
            .ToList();
    }
}
