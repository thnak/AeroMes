using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetCompatibleMoldsForMachine;

public record GetCompatibleMoldsForMachineQuery(string MachineCode)
    : IQuery<IReadOnlyList<MoldCompatibilityDto>>;
