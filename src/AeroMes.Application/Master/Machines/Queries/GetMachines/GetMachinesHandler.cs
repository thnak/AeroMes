using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Machines.Queries.GetMachines;

public class GetMachinesHandler(IMachineRepository repo)
    : IRequestHandler<GetMachinesQuery, IReadOnlyList<MachineDto>>
{
    public async Task<IReadOnlyList<MachineDto>> Handle(GetMachinesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new MachineDto(
            x.MachineCode,
            x.MachineName,
            x.WorkCenterID,
            x.WorkCenter?.WorkCenterName,
            x.Brand,
            x.Model,
            x.Status.ToString(),
            x.IsActive)).ToList();
    }
}
