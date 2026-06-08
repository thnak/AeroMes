using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;

public record GetStorageLocationsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<StorageLocationDto>>;

public record StorageLocationDto(
    int LocationID,
    string LocationCode,
    string LocationName,
    string LocationType,
    int? WorkCenterID,
    string? WorkCenterName,
    bool IsActive);
