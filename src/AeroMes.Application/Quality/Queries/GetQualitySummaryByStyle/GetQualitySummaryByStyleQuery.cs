using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetQualitySummaryByStyle;

public record GetQualitySummaryByStyleQuery(
    string StyleCode,
    DateTime FromDate,
    DateTime ToDate) : IQuery<QualitySummaryByStyleDto?>;
