using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.BomItems.Queries.GetBomItems;

public class GetBomItemsHandler(IBomItemRepository repo)
    : IQueryHandler<GetBomItemsQuery, IReadOnlyList<BomItemDto>>
{
    public async Task<IReadOnlyList<BomItemDto>> HandleAsync(GetBomItemsQuery q, CancellationToken ct)
    {
        var items = await repo.GetByParentAsync(q.ParentProductCode, ct);
        return items.Select(x => new BomItemDto(
            x.BomID,
            x.ParentProductCode,
            x.ChildProductCode,
            x.RequiredQty,
            x.ScrapFactor,
            x.IsActive)).ToList();
    }
}
