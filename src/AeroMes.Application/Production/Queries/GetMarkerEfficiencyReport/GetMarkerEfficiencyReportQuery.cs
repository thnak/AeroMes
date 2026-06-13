using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetMarkerEfficiencyReport;

public record GetMarkerEfficiencyReportQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? StyleCode = null) : IQuery<IReadOnlyList<MarkerEfficiencyReportDto>>;
