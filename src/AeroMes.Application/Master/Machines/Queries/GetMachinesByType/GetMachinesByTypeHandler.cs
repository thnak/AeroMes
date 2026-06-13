using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Machines.Queries.GetMachinesByType;

public sealed class GetMachinesByTypeHandler(IMachineRepository machines)
    : IQueryHandler<GetMachinesByTypeQuery, IReadOnlyList<MachinesByTypeDto>>
{
    public async Task<IReadOnlyList<MachinesByTypeDto>> HandleAsync(GetMachinesByTypeQuery query, CancellationToken ct)
    {
        var list = await machines.GetByTypeAsync(query.MachineType, query.ActiveOnly, ct);
        return list.Select(m => new MachinesByTypeDto(
            m.MachineCode, m.MachineName, m.MachineType, m.WorkCenterID,
            m.WorkCenter?.WorkCenterName, m.Status.ToString(), m.IsActive,
            m.CustomAttributes, m.ClampingForceTons, m.SewingMachineClass)).ToList();
    }
}
