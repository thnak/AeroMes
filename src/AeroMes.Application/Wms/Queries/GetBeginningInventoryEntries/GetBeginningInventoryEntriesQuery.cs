using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBeginningInventoryEntries;

public record GetBeginningInventoryEntriesQuery(
    int? WarehouseId,
    string? ProductCode,
    DateOnly? Period
) : IQuery<IReadOnlyList<BeginningInventoryEntryDto>>;

public record BeginningInventoryEntryDto(
    int EntryId,
    DateOnly Period,
    int WarehouseId,
    string? WarehouseName,
    string ProductCode,
    string UnitOfMeasure,
    decimal BeginningQuantity,
    string? LotNumber,
    DateOnly? ExpirationDate,
    DateTime CreatedAt,
    string? CreatedBy);
