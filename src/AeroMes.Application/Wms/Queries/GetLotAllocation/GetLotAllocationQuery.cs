using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetLotAllocation;

public record GetLotAllocationQuery(
    string ProductCode,
    decimal RequiredQty,
    int? LocationId,
    PickingStrategy? StrategyOverride) : IQuery<AllocationResult>;
