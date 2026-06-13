using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialSupplyRequests;

public class GetMaterialSupplyRequestsHandler(IMaterialSupplyRequestRepository repo)
    : IQueryHandler<GetMaterialSupplyRequestsQuery, IReadOnlyList<MaterialSupplyRequestSummaryDto>>
{
    public async Task<IReadOnlyList<MaterialSupplyRequestSummaryDto>> HandleAsync(
        GetMaterialSupplyRequestsQuery query, CancellationToken ct)
    {
        var requests = await repo.GetAllAsync(query.RequestType, query.Status, ct);
        return [.. requests.Select(r => new MaterialSupplyRequestSummaryDto(
            r.RequestId,
            r.VoucherNumber,
            r.RequestType,
            r.Status,
            r.RequesterUnit,
            r.RequiredByDate,
            r.Notes,
            r.Lines.Count,
            r.CreatedAt,
            r.CreatedBy))];
    }
}
