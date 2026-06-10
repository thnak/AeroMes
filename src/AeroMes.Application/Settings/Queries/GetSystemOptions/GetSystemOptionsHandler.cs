using AeroMes.Application.Interfaces;
using AeroMes.Domain.Settings;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Settings.Queries.GetSystemOptions;

public class GetSystemOptionsHandler(ISystemOptionsRepository repo)
    : IQueryHandler<GetSystemOptionsQuery, SystemOptions>
{
    public Task<SystemOptions> HandleAsync(GetSystemOptionsQuery q, CancellationToken ct)
        => repo.GetAsync(ct);
}
