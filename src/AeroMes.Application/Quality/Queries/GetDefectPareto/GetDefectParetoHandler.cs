using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetDefectPareto;

public class GetDefectParetoHandler(IInlineInspectionRepository repo)
    : IQueryHandler<GetDefectParetoQuery, IReadOnlyList<DefectParetoDto>>
{
    public Task<IReadOnlyList<DefectParetoDto>> HandleAsync(GetDefectParetoQuery query, CancellationToken ct)
        => repo.GetDefectParetoAsync(query.StyleCode, query.WorkCenterID, query.FromDate, query.ToDate, query.TopN, ct);
}
