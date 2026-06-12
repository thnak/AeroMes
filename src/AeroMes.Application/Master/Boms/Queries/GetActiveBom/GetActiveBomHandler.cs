using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetActiveBom;

public class GetActiveBomHandler(IBomHeaderRepository repo)
    : IQueryHandler<GetActiveBomQuery, BomVersionDetailDto?>
{
    public async Task<BomVersionDetailDto?> HandleAsync(GetActiveBomQuery query, CancellationToken ct)
    {
        var header = await repo.GetActiveByProductWithDetailsAsync(query.ProductCode, ct);
        return header is null ? null : ToDetailDto(header);
    }

    internal static BomVersionDetailDto ToDetailDto(BomHeader h) => new(
        h.BomHeaderId, h.ProductCode, h.Version, h.Status.ToString(),
        h.EffectiveFrom, h.EffectiveTo, h.BaseQuantity,
        h.EcoReference, h.ApprovedBy, h.ApprovedAt, h.Notes,
        h.Lines
            .OrderBy(l => l.LineNo)
            .Select(l => new BomLineDto(
                l.BomLineId, l.LineNo, l.ComponentCode, l.Component?.ProductName,
                l.RequiredQty, l.UoMCode, l.ScrapFactor, l.IsPhantom, l.Notes))
            .ToList());
}
