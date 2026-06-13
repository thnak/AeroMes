using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBeginningInventoryEntries;

public class GetBeginningInventoryEntriesHandler(IBeginningInventoryEntryRepository repo)
    : IQueryHandler<GetBeginningInventoryEntriesQuery, IReadOnlyList<BeginningInventoryEntryDto>>
{
    public async Task<IReadOnlyList<BeginningInventoryEntryDto>> HandleAsync(
        GetBeginningInventoryEntriesQuery q, CancellationToken ct)
    {
        var entries = await repo.GetAllAsync(q.WarehouseId, q.ProductCode, q.Period, ct);
        return [.. entries.Select(e => new BeginningInventoryEntryDto(
            e.EntryId,
            e.Period,
            e.WarehouseId,
            e.Warehouse?.WarehouseName,
            e.ProductCode,
            e.UnitOfMeasure,
            e.BeginningQuantity,
            e.LotNumber,
            e.ExpirationDate,
            e.CreatedAt,
            e.CreatedBy))];
    }
}
