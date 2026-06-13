using AeroMes.Application.Wms.Services;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetLotAllocation;

public class GetLotAllocationHandler(ILotAllocationService allocationService)
    : IQueryHandler<GetLotAllocationQuery, AllocationResult>
{
    public Task<AllocationResult> HandleAsync(GetLotAllocationQuery query, CancellationToken ct)
        => allocationService.AllocateAsync(
            query.ProductCode,
            query.RequiredQty,
            query.LocationId,
            query.StrategyOverride,
            ct);
}
