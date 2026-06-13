using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineCostRates;

public class GetMachineCostRatesHandler(IMachineCostRateRepository repository)
    : IQueryHandler<GetMachineCostRatesQuery, IReadOnlyList<MachineCostRateDto>>
{
    public Task<IReadOnlyList<MachineCostRateDto>> HandleAsync(GetMachineCostRatesQuery query, CancellationToken ct)
        => repository.GetByMachineAsync(query.MachineCode, query.IncludeExpired, ct);
}
