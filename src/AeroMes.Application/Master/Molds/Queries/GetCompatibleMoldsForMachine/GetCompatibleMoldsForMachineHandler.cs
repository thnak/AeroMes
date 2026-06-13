using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetCompatibleMoldsForMachine;

public class GetCompatibleMoldsForMachineHandler(IMoldCompatibilityRepository repo)
    : IQueryHandler<GetCompatibleMoldsForMachineQuery, IReadOnlyList<MoldCompatibilityDto>>
{
    public Task<IReadOnlyList<MoldCompatibilityDto>> HandleAsync(
        GetCompatibleMoldsForMachineQuery query, CancellationToken ct)
        => repo.GetCompatibleMoldsAsync(query.MachineCode, ct);
}
