using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.CompareBomVersions;

public class CompareBomVersionsHandler(IBomHeaderRepository repo)
    : IQueryHandler<CompareBomVersionsQuery, BomCompareDto>
{
    public async Task<BomCompareDto> HandleAsync(CompareBomVersionsQuery query, CancellationToken ct)
    {
        var from = await repo.GetByProductAndVersionWithDetailsAsync(query.ProductCode, query.FromVersion, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{query.ProductCode}/{query.FromVersion}");
        var to = await repo.GetByProductAndVersionWithDetailsAsync(query.ProductCode, query.ToVersion, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{query.ProductCode}/{query.ToVersion}");

        // Diff is by component: a component appearing on several lines is aggregated per line set,
        // so versions are compared on the first line per component.
        var fromByComponent = from.Lines
            .GroupBy(l => l.ComponentCode)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.LineNo).First());
        var toByComponent = to.Lines
            .GroupBy(l => l.ComponentCode)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.LineNo).First());

        var added = toByComponent.Values
            .Where(l => !fromByComponent.ContainsKey(l.ComponentCode))
            .OrderBy(l => l.LineNo)
            .Select(ToLineDto)
            .ToList();

        var removed = fromByComponent.Values
            .Where(l => !toByComponent.ContainsKey(l.ComponentCode))
            .OrderBy(l => l.LineNo)
            .Select(ToLineDto)
            .ToList();

        var changed = toByComponent.Values
            .Where(l => fromByComponent.ContainsKey(l.ComponentCode))
            .Select(l => (Old: fromByComponent[l.ComponentCode], New: l))
            .Where(p => p.Old.RequiredQty != p.New.RequiredQty
                        || p.Old.UoMCode != p.New.UoMCode
                        || p.Old.ScrapFactor != p.New.ScrapFactor)
            .OrderBy(p => p.New.LineNo)
            .Select(p => new BomLineChangeDto(
                p.New.ComponentCode, p.New.Component?.ProductName,
                p.Old.RequiredQty, p.New.RequiredQty,
                p.Old.UoMCode, p.New.UoMCode,
                p.Old.ScrapFactor, p.New.ScrapFactor))
            .ToList();

        return new BomCompareDto(
            from.ProductCode, from.Version, to.Version, added, removed, changed);
    }

    private static BomLineDto ToLineDto(BomLine l) => new(
        l.BomLineId, l.LineNo, l.ComponentCode, l.Component?.ProductName,
        l.RequiredQty, l.UoMCode, l.ScrapFactor, l.IsPhantom, l.Notes);
}
