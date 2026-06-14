using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetOeeByMachine;

public record GetOeeByMachineQuery(DateOnly From, DateOnly To) : IQuery<IReadOnlyList<OeeByMachineDto>>;

public class GetOeeByMachineQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetOeeByMachineQuery, IReadOnlyList<OeeByMachineDto>>
{
    public Task<IReadOnlyList<OeeByMachineDto>> HandleAsync(GetOeeByMachineQuery query, CancellationToken ct = default)
        => repo.GetOeeByMachineAsync(query.From, query.To, ct);
}
