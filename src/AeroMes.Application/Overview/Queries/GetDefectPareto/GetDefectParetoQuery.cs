using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetDefectPareto;

public record GetDefectParetoQuery(DateOnly From, DateOnly To, string? ProductCode = null)
    : IQuery<IReadOnlyList<DefectParetoItemDto>>;

public class GetDefectParetoQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetDefectParetoQuery, IReadOnlyList<DefectParetoItemDto>>
{
    public Task<IReadOnlyList<DefectParetoItemDto>> HandleAsync(GetDefectParetoQuery query, CancellationToken ct = default)
        => repo.GetDefectParetoAsync(query.From, query.To, query.ProductCode, ct);
}
