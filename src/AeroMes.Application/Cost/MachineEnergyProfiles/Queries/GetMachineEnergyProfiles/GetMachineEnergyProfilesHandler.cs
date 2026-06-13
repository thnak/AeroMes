using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineEnergyProfiles.Queries.GetMachineEnergyProfiles;

public class GetMachineEnergyProfilesHandler(IMachineEnergyProfileRepository repository)
    : IQueryHandler<GetMachineEnergyProfilesQuery, IReadOnlyList<MachineEnergyProfileDto>>
{
    public Task<IReadOnlyList<MachineEnergyProfileDto>> HandleAsync(
        GetMachineEnergyProfilesQuery query, CancellationToken ct)
        => repository.GetByMachineAsync(query.MachineCode, ct);
}
