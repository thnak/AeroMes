using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Inventory.Queries.GetLotTrace;

public class GetLotTraceHandler(IInventoryStockRepository repo)
    : IQueryHandler<GetLotTraceQuery, LotTraceDto>
{
    public async Task<LotTraceDto> HandleAsync(GetLotTraceQuery q, CancellationToken ct)
    {
        var entries = await repo.GetByLotNumberAsync(q.LotNumber, ct);

        var stockEntries = entries.Select(s => new LotStockEntryDto(
            s.StockID,
            s.LocationID,
            s.StorageLocation!.LocationCode,
            s.StorageLocation.LocationName,
            s.StorageLocation.LocationType.ToString(),
            s.StorageLocation.WorkCenter?.WorkCenterCode,
            s.ProductCode,
            s.Quantity,
            s.UpdatedAt)).ToList();

        return new LotTraceDto(q.LotNumber, stockEntries);
    }
}
