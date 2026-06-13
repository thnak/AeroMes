using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Machines.Queries.GetCompatibleMachinesForMold;

public sealed class GetCompatibleMachinesForMoldHandler(IMachineRepository machines)
    : IQueryHandler<GetCompatibleMachinesForMoldQuery, IReadOnlyList<CompatibleMachineDto>>
{
    public async Task<IReadOnlyList<CompatibleMachineDto>> HandleAsync(
        GetCompatibleMachinesForMoldQuery query, CancellationToken ct)
    {
        var list = await machines.GetCompatibleForMoldAsync(query.MinClampingForceTons, ct);
        return list.Select(m => new CompatibleMachineDto(
            m.MachineCode, m.MachineName, m.MachineType, m.WorkCenterID,
            m.WorkCenter?.WorkCenterName, m.Status.ToString(),
            m.ClampingForceTons!.Value)).ToList();
    }
}
