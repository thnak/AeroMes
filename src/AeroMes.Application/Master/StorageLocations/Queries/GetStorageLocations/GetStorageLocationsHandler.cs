using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;

public class GetStorageLocationsHandler(IStorageLocationRepository repo)
    : IRequestHandler<GetStorageLocationsQuery, IReadOnlyList<StorageLocationDto>>
{
    public async Task<IReadOnlyList<StorageLocationDto>> Handle(GetStorageLocationsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new StorageLocationDto(
            x.LocationID,
            x.LocationCode,
            x.LocationName,
            x.LocationType.ToString(),
            x.WorkCenterID,
            x.WorkCenter?.WorkCenterName,
            x.IsActive)).ToList();
    }
}
