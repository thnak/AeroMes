using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Queries.GetEnergyTariffs;

public class GetEnergyTariffsHandler(IEnergyTariffRepository repository)
    : IQueryHandler<GetEnergyTariffsQuery, IReadOnlyList<EnergyTariffDto>>
{
    public Task<IReadOnlyList<EnergyTariffDto>> HandleAsync(GetEnergyTariffsQuery query, CancellationToken ct)
        => repository.GetListAsync(query.IncludeInactive, ct);
}
