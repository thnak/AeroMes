using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetStockStatus;

public record GetStockStatusQuery(int? LocationId)
    : IQuery<IReadOnlyList<StockStatusItemDto>>;

public record StockStatusItemDto(
    int PolicyId,
    string ProductCode,
    int LocationId,
    decimal CurrentQty,
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    string StockLevel);
