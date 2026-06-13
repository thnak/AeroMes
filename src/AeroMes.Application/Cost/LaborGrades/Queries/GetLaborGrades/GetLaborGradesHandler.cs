using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.LaborGrades.Queries.GetLaborGrades;

public class GetLaborGradesHandler(ILaborGradeRepository repository)
    : IQueryHandler<GetLaborGradesQuery, IReadOnlyList<LaborGradeDto>>
{
    public Task<IReadOnlyList<LaborGradeDto>> HandleAsync(GetLaborGradesQuery query, CancellationToken ct)
        => repository.GetListAsync(query.Keyword, query.IncludeExpired, ct);
}
