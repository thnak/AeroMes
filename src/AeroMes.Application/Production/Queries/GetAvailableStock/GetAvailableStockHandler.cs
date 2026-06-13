using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetAvailableStock;

public class GetAvailableStockHandler(
    IInventoryStockRepository repo) : IQueryHandler<GetAvailableStockQuery, IReadOnlyList<AvailableStockDto>>
{
    public async Task<IReadOnlyList<AvailableStockDto>> HandleAsync(GetAvailableStockQuery q, CancellationToken ct)
    {
        var stocks = await repo.GetFilteredAsync(null, q.ProductCode, ct);

        return stocks
            .Where(s => q.LocationId is null || s.LocationID == q.LocationId)
            .Where(s => s.AvailableQty > 0)
            .Select(s => new AvailableStockDto(
                s.StockID,
                s.LocationID,
                s.ProductCode,
                s.LotNumber,
                s.Quantity,
                s.ReservedQty,
                s.AvailableQty,
                s.SecondaryQty,
                s.ExpiryDate,
                s.ManufacturedDate,
                s.ReceivedAt))
            .OrderBy(s => s.ExpiryDate ?? DateOnly.MaxValue)
            .ThenBy(s => s.ReceivedAt)
            .ToList();
    }
}
