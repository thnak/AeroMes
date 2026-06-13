using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetDHUTrend;

public record GetDHUTrendQuery(
    int? WOID,
    int? WorkCenterID,
    string? StyleCode,
    DateTime FromDate,
    DateTime ToDate) : IQuery<IReadOnlyList<DHUTrendDto>>;
