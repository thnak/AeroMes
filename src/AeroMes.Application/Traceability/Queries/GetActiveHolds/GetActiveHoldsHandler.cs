using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetActiveHolds;

public sealed class GetActiveHoldsHandler(ILotHoldRepository repository)
    : IQueryHandler<GetActiveHoldsQuery, PagedResult<LotHoldDto>>
{
    public async Task<PagedResult<LotHoldDto>> HandleAsync(
        GetActiveHoldsQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetActiveHoldsAsync(
            query.LotNumber, query.HoldReason, query.Page, query.PageSize, ct);
        return new PagedResult<LotHoldDto>(items, total, query.Page, query.PageSize);
    }
}
