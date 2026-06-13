using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOutputByProductReport;

public class GetOutputByProductReportHandler(IProductionLogRepository repo)
    : IQueryHandler<GetOutputByProductReportQuery, IReadOnlyList<ProductOutputDto>>
{
    public Task<IReadOnlyList<ProductOutputDto>> HandleAsync(
        GetOutputByProductReportQuery q, CancellationToken ct = default)
        => repo.GetOutputByProductAsync(q.From, q.To, ct);
}
