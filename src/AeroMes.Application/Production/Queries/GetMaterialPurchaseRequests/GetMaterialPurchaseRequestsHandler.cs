using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetMaterialPurchaseRequests;

public class GetMaterialPurchaseRequestsHandler(IMaterialPurchaseRequestRepository repo)
    : IQueryHandler<GetMaterialPurchaseRequestsQuery, PagedResult<MaterialPurchaseRequestDto>>
{
    public async Task<PagedResult<MaterialPurchaseRequestDto>> HandleAsync(
        GetMaterialPurchaseRequestsQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetListAsync(
            query.Status, query.SourceType, query.RequestingUnit,
            query.From, query.To, query.Page, query.PageSize, ct);
        return new PagedResult<MaterialPurchaseRequestDto>(items, total, query.Page, query.PageSize);
    }
}
