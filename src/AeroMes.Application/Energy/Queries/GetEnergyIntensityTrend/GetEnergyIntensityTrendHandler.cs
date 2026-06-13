using AeroMes.Domain.Energy.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Energy.Queries.GetEnergyIntensityTrend;

public class GetEnergyIntensityTrendHandler(IEnergyRepository repository)
    : IQueryHandler<GetEnergyIntensityTrendQuery, IReadOnlyList<EnergyIntensityTrendDto>>
{
    public Task<IReadOnlyList<EnergyIntensityTrendDto>> HandleAsync(
        GetEnergyIntensityTrendQuery query, CancellationToken ct)
        => repository.GetIntensityTrendAsync(query.MachineCode, query.Months, ct);
}
