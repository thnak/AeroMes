using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetInventorySnapshot;

public class GetInventorySnapshotHandler(IInventoryStockRepository stockRepo)
    : IQueryHandler<GetInventorySnapshotQuery, InventorySnapshotDto>
{
    public async Task<InventorySnapshotDto> HandleAsync(GetInventorySnapshotQuery q, CancellationToken ct)
    {
        var stocks = await stockRepo.GetFilteredAsync(q.LocationType, productCode: null, ct);

        var rows = stocks
            .OrderBy(s => s.StorageLocation?.LocationType.ToString())
            .ThenBy(s => s.ProductCode)
            .Select(s => new InventorySnapshotRowDto(
                s.ProductCode,
                s.StorageLocation?.LocationName,
                s.StorageLocation?.LocationType.ToString(),
                s.LotNumber,
                s.Quantity,
                s.ReservedQty,
                s.AvailableQty,
                s.UpdatedAt))
            .ToList();

        return new InventorySnapshotDto(DateTime.UtcNow, rows.Count, rows);
    }
}
