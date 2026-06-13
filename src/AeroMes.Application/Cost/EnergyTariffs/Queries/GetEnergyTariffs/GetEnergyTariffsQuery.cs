using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Queries.GetEnergyTariffs;

public record GetEnergyTariffsQuery(bool IncludeInactive = false)
    : IQuery<IReadOnlyList<EnergyTariffDto>>;
