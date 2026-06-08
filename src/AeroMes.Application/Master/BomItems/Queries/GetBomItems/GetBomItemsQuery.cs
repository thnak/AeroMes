using MediatR;

namespace AeroMes.Application.Master.BomItems.Queries.GetBomItems;

public record GetBomItemsQuery(string ParentProductCode) : IRequest<IReadOnlyList<BomItemDto>>;

public record BomItemDto(
    int BomID,
    string ParentProductCode,
    string ChildProductCode,
    decimal RequiredQty,
    decimal ScrapFactor,
    bool IsActive);
