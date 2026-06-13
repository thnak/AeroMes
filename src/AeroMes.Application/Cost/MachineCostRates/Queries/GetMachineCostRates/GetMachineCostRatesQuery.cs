using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineCostRates;

public record GetMachineCostRatesQuery(string MachineCode, bool IncludeExpired = false)
    : IQuery<IReadOnlyList<MachineCostRateDto>>;
