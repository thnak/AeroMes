using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOrderProgressReport;

public class GetOrderProgressReportHandler(IProductionOrderRepository repo)
    : IQueryHandler<GetOrderProgressReportQuery, IReadOnlyList<OrderProgressDto>>
{
    public Task<IReadOnlyList<OrderProgressDto>> HandleAsync(
        GetOrderProgressReportQuery q, CancellationToken ct = default)
        => repo.GetProgressReportAsync(q.From, q.To, q.Status, ct);
}
