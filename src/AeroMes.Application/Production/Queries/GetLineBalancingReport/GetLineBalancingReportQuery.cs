using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetLineBalancingReport;

public record GetLineBalancingReportQuery(
    int? WorkCenterID = null,
    string? StyleCode = null) : IQuery<IReadOnlyList<LineBalancingDto>>;
