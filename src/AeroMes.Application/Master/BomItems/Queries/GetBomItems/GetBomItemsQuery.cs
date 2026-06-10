using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.BomItems.Queries.GetBomItems;

public record GetBomItemsQuery(string ParentProductCode) : IQuery<IReadOnlyList<BomItemDto>>;

public record BomItemDto(
    int BomID,
    string ParentProductCode,
    string ChildProductCode,
    decimal RequiredQty,
    decimal ScrapFactor,
    bool IsActive);
