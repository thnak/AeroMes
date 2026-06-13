using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.LaborGrades.Queries.GetLaborGrades;

public record GetLaborGradesQuery(string? Keyword, bool IncludeExpired = false)
    : IQuery<IReadOnlyList<LaborGradeDto>>;
