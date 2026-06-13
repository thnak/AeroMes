using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineEnergyProfiles.Queries.GetMachineEnergyProfiles;

public record GetMachineEnergyProfilesQuery(string MachineCode)
    : IQuery<IReadOnlyList<MachineEnergyProfileDto>>;
