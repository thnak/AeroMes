using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetScrapPareto;

public record GetScrapParetoQuery(DateTime From, DateTime To, int? WorkCenterId)
    : IQuery<IReadOnlyList<ScrapParetoDto>>;
