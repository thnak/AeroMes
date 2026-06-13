using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialsInLot;

public class GetSerialsInLotHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetSerialsInLotQuery, PagedResult<SerialUnitDto>>
{
    public async Task<PagedResult<SerialUnitDto>> HandleAsync(GetSerialsInLotQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetByLotAsync(query.LotNumber, query.Page, query.PageSize, ct);
        return new PagedResult<SerialUnitDto>(items, total, query.Page, query.PageSize);
    }
}
