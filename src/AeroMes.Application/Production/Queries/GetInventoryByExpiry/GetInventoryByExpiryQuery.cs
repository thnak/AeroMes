using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetInventoryByExpiry;

public record GetInventoryByExpiryQuery(
    int DaysToExpiry,
    int? LocationId = null) : IQuery<IReadOnlyList<ExpiringStockDto>>;

public record ExpiringStockDto(
    long StockId,
    int LocationId,
    string ProductCode,
    string LotNumber,
    decimal AvailableQty,
    DateOnly ExpiryDate,
    int DaysRemaining);
