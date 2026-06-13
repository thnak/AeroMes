using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequests;

public class GetFinishedProductIntakeRequestsHandler(IFinishedProductIntakeRequestRepository repo)
    : IQueryHandler<GetFinishedProductIntakeRequestsQuery, IReadOnlyList<FinishedProductIntakeRequestSummaryDto>>
{
    public async Task<IReadOnlyList<FinishedProductIntakeRequestSummaryDto>> HandleAsync(
        GetFinishedProductIntakeRequestsQuery query, CancellationToken ct)
    {
        var requests = await repo.GetAllAsync(query.IntakePurpose, query.Status, query.ProductionOrderId, ct);
        return [.. requests.Select(r => new FinishedProductIntakeRequestSummaryDto(
            r.IntakeRequestId,
            r.RequestNumber,
            r.IntakePurpose,
            r.WarehouseType,
            r.Status,
            r.ProductionOrderId,
            r.RequesterUnit,
            r.RequestDate,
            r.SentAt,
            r.Notes,
            r.Lines.Count,
            r.CreatedAt,
            r.CreatedBy))];
    }
}
