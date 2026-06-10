using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Queries.GetMachineProductConfigs;

public class GetMachineProductConfigsHandler(IMachineProductConfigRepository repo)
    : IQueryHandler<GetMachineProductConfigsQuery, IReadOnlyList<MachineProductConfigDto>>
{
    public async Task<IReadOnlyList<MachineProductConfigDto>> HandleAsync(GetMachineProductConfigsQuery q, CancellationToken ct)
    {
        var items = await repo.GetByMachineAsync(q.MachineCode, ct);
        return items.Select(x => new MachineProductConfigDto(
            x.MachineCode, x.ProductCode, x.RoutingStepId,
            x.IdealCycleTimeSeconds, x.TargetThroughputPerHour,
            x.SetupTimeSeconds, x.EffectiveFrom)).ToList();
    }
}
