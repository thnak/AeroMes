using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;

public record GetStorageLocationsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<StorageLocationDto>>;

public record StorageLocationDto(
    int LocationID,
    string LocationCode,
    string LocationName,
    string LocationType,
    int? WorkCenterID,
    string? WorkCenterName,
    bool IsActive);
