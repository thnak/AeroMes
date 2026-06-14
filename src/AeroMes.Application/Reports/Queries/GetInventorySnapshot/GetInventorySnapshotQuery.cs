using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetInventorySnapshot;

public record GetInventorySnapshotQuery(LocationType? LocationType) : IQuery<InventorySnapshotDto>;

public record InventorySnapshotDto(
    DateTime AsOf,
    int TotalLines,
    IReadOnlyList<InventorySnapshotRowDto> Rows);

public record InventorySnapshotRowDto(
    string ProductCode,
    string? LocationName,
    string? LocationType,
    string LotNumber,
    decimal Quantity,
    decimal ReservedQty,
    decimal AvailableQty,
    DateTime UpdatedAt);
