using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetBundleLocation;

public class GetBundleLocationHandler(IBundleRepository repo)
    : IQueryHandler<GetBundleLocationQuery, BundleLocationDto?>
{
    public Task<BundleLocationDto?> HandleAsync(GetBundleLocationQuery query, CancellationToken ct)
        => repo.GetLocationAsync(query.BundleBarcode, ct);
}
