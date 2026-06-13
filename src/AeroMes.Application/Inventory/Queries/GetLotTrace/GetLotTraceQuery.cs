using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Inventory.Queries.GetLotTrace;

public record GetLotTraceQuery(string LotNumber) : IQuery<LotTraceDto>;

public record LotTraceDto(
    string LotNumber,
    IReadOnlyList<LotStockEntryDto> StockEntries);

public record LotStockEntryDto(
    long StockID,
    int LocationID,
    string LocationCode,
    string LocationName,
    string LocationType,
    string? WorkCenterCode,
    string ProductCode,
    decimal Quantity,
    DateTime UpdatedAt);
