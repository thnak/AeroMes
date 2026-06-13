using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineTotalRate;

public class GetMachineTotalRateHandler(IMachineCostRateRepository repository)
    : IQueryHandler<GetMachineTotalRateQuery, MachineTotalRateDto>
{
    public Task<MachineTotalRateDto> HandleAsync(GetMachineTotalRateQuery query, CancellationToken ct)
        => repository.GetTotalRateAsync(query.MachineCode, ct);
}
