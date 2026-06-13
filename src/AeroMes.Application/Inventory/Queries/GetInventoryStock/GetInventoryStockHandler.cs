using AeroMes.Domain.Master;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Inventory.Queries.GetInventoryStock;

public class GetInventoryStockHandler(IInventoryStockRepository repo)
    : IQueryHandler<GetInventoryStockQuery, IReadOnlyList<InventoryStockDto>>
{
    public async Task<IReadOnlyList<InventoryStockDto>> HandleAsync(
        GetInventoryStockQuery q, CancellationToken ct)
    {
        LocationType? locationType = null;
        if (q.LocationType is not null && Enum.TryParse<LocationType>(q.LocationType, true, out var lt))
            locationType = lt;

        var stocks = await repo.GetFilteredAsync(locationType, q.ProductCode, ct);

        return [.. stocks.Select(s => new InventoryStockDto(
            s.StockID,
            s.LocationID,
            s.StorageLocation!.LocationCode,
            s.StorageLocation.LocationName,
            s.StorageLocation.LocationType.ToString(),
            s.StorageLocation.WorkCenter?.WorkCenterCode,
            s.ProductCode,
            s.LotNumber,
            s.Quantity,
            s.UpdatedAt))];
    }
}
