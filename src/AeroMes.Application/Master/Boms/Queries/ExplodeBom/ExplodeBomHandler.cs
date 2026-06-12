using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.ExplodeBom;

/// <summary>
/// Multi-level explosion over Active BOM versions: breadth-first by level,
/// batching one repository call per level. Scrap factors compound down the tree;
/// cycles are cut per-branch and reported as a domain error.
/// </summary>
public class ExplodeBomHandler(IBomHeaderRepository repo)
    : IQueryHandler<ExplodeBomQuery, IReadOnlyList<ExplodedBomLineDto>>
{
    private const int MaxDepth = 20;

    public async Task<IReadOnlyList<ExplodedBomLineDto>> HandleAsync(
        ExplodeBomQuery query, CancellationToken ct)
    {
        var rootCode = query.ProductCode.Trim().ToUpperInvariant();
        var root = await repo.GetActiveByProductWithDetailsAsync(rootCode, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), rootCode);

        var result = new List<ExplodedBomLineDto>();
        // (parentCode, header, multiplier = parent units to produce, ancestor path for cycle detection)
        var frontier = new List<(string ParentCode, BomHeader Header, decimal Multiplier, HashSet<string> Path)>
        {
            (rootCode, root, query.Quantity, new HashSet<string> { rootCode }),
        };

        for (var level = 1; frontier.Count > 0; level++)
        {
            if (level > MaxDepth)
                throw new DomainException($"BOM của '{rootCode}' vượt quá độ sâu tối đa {MaxDepth} cấp.");

            var componentCodes = frontier
                .SelectMany(f => f.Header.Lines.Select(l => l.ComponentCode))
                .Distinct()
                .ToList();
            var childHeaders = (await repo.GetActiveForProductsAsync(componentCodes, ct))
                .ToDictionary(h => h.ProductCode);

            var next = new List<(string, BomHeader, decimal, HashSet<string>)>();
            foreach (var (parentCode, header, multiplier, path) in frontier)
            {
                foreach (var line in header.Lines.OrderBy(l => l.LineNo))
                {
                    if (path.Contains(line.ComponentCode))
                        throw new DomainException(
                            $"BOM của '{rootCode}' chứa vòng lặp tại nguyên liệu '{line.ComponentCode}'.");

                    var perParent = line.RequiredQty / header.BaseQuantity;
                    var total = perParent * multiplier * (1 + line.ScrapFactor / 100m);
                    var hasChildBom = childHeaders.TryGetValue(line.ComponentCode, out var childHeader);

                    result.Add(new ExplodedBomLineDto(
                        level, parentCode, line.ComponentCode, line.Component?.ProductName,
                        perParent, decimal.Round(total, 6), line.UoMCode,
                        line.ScrapFactor, line.IsPhantom, hasChildBom));

                    if (hasChildBom)
                        next.Add((line.ComponentCode, childHeader!, total,
                            [.. path, line.ComponentCode]));
                }
            }
            frontier = next;
        }

        return result;
    }
}
