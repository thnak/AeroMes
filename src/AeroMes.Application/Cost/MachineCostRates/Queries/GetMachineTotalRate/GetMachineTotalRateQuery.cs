using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineTotalRate;

public record GetMachineTotalRateQuery(string MachineCode) : IQuery<MachineTotalRateDto>;
