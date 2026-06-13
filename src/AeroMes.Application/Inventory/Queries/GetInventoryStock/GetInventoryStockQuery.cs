using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Inventory.Queries.GetInventoryStock;

public record GetInventoryStockQuery(string? LocationType, string? ProductCode)
    : IQuery<IReadOnlyList<InventoryStockDto>>;

public record InventoryStockDto(
    long StockID,
    int LocationID,
    string LocationCode,
    string LocationName,
    string LocationType,
    string? WorkCenterCode,
    string ProductCode,
    string LotNumber,
    decimal Quantity,
    DateTime UpdatedAt);
