using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Queries.GetMachineSetupSheet;

public class GetMachineSetupSheetHandler(IMachineProductParamRepository repo)
    : IQueryHandler<GetMachineSetupSheetQuery, IReadOnlyList<MachineProductParamDto>>
{
    public async Task<IReadOnlyList<MachineProductParamDto>> HandleAsync(GetMachineSetupSheetQuery q, CancellationToken ct)
    {
        var items = q.ProductCode is not null
            ? await repo.GetByMachineAndProductAsync(q.MachineCode, q.ProductCode, ct)
            : await repo.GetByMachineAsync(q.MachineCode, ct);

        return items
            .OrderBy(x => x.ProductCode)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.ParamName)
            .Select(x => new MachineProductParamDto(
                x.MachineCode, x.ProductCode, x.ParamName,
                x.Unit, x.NominalValue, x.MinValue, x.MaxValue,
                x.IsControlParam, x.DisplayOrder))
            .ToList();
    }
}
