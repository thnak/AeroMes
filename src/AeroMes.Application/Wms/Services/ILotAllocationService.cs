using AeroMes.Domain.Master;

namespace AeroMes.Application.Wms.Services;

public interface ILotAllocationService
{
    Task<AllocationResult> AllocateAsync(
        string productCode,
        decimal requiredQty,
        int? locationId,
        PickingStrategy? strategyOverride,
        CancellationToken ct = default);
}

public record AllocationResult(
    IReadOnlyList<LotAllocation> Allocations,
    bool IsFulfilled,
    decimal AllocatedQty,
    decimal RequiredQty,
    IReadOnlyList<RejectedLot> RejectedLots);

public record LotAllocation(
    string LotNumber,
    int LocationId,
    int? BinId,
    decimal AllocatedQty,
    DateOnly? ExpiryDate);

public record RejectedLot(
    string LotNumber,
    decimal AvailableQty,
    DateOnly? ExpiryDate,
    string RejectReason);
