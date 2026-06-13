using AeroMes.Domain.Energy.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Energy.Queries.GetEnergyIntensityTrend;

public record GetEnergyIntensityTrendQuery(string MachineCode, int Months = 6)
    : IQuery<IReadOnlyList<EnergyIntensityTrendDto>>;
