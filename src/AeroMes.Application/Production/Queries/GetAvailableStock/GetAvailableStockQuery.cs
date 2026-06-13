using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetAvailableStock;

public record GetAvailableStockQuery(
    string ProductCode,
    int? LocationId = null) : IQuery<IReadOnlyList<AvailableStockDto>>;

public record AvailableStockDto(
    long StockId,
    int LocationId,
    string ProductCode,
    string LotNumber,
    decimal Quantity,
    decimal ReservedQty,
    decimal AvailableQty,
    decimal SecondaryQty,
    DateOnly? ExpiryDate,
    DateOnly? ManufacturedDate,
    DateTime ReceivedAt);
