using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetMarkerEfficiencyReport;

public class GetMarkerEfficiencyReportHandler(ICutOrderRepository repo)
    : IQueryHandler<GetMarkerEfficiencyReportQuery, IReadOnlyList<MarkerEfficiencyReportDto>>
{
    public Task<IReadOnlyList<MarkerEfficiencyReportDto>> HandleAsync(
        GetMarkerEfficiencyReportQuery query, CancellationToken ct)
        => repo.GetMarkerEfficiencyReportAsync(query.FromDate, query.ToDate, query.StyleCode, ct);
}
