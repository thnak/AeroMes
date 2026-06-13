using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetDefectPareto;

public record GetDefectParetoQuery(
    string? StyleCode,
    int? WorkCenterID,
    DateTime FromDate,
    DateTime ToDate,
    int TopN = 10) : IQuery<IReadOnlyList<DefectParetoDto>>;
