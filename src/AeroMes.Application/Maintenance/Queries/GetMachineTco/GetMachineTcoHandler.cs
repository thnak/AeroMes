using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Maintenance.Queries.GetMachineTco;

public class GetMachineTcoHandler(IMachineTcoRepository repository)
    : IQueryHandler<GetMachineTcoQuery, IReadOnlyList<MachineTcoDto>>
{
    public Task<IReadOnlyList<MachineTcoDto>> HandleAsync(GetMachineTcoQuery query, CancellationToken ct)
        => repository.GetTcoHistoryAsync(query.MachineCode, query.Months, ct);
}
