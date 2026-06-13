using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetSoProductionStatusReport;

public class GetSoProductionStatusReportHandler(IProductionOrderRepository repo)
    : IQueryHandler<GetSoProductionStatusReportQuery, IReadOnlyList<SoProductionStatusDto>>
{
    public Task<IReadOnlyList<SoProductionStatusDto>> HandleAsync(
        GetSoProductionStatusReportQuery q, CancellationToken ct = default)
        => repo.GetSoProductionStatusAsync(q.From, q.To, ct);
}
