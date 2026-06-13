namespace AeroMes.Domain.Production.Repositories;

public record BundleLocationDto(
    int BundleID,
    string BundleBarcode,
    int CutOrderID,
    string StyleCode,
    string ColorCode,
    string SizeCode,
    int Quantity,
    string? CurrentOperationCode,
    int? CurrentWorkCenterID,
    string Status,
    int QtyOKCumulative,
    int QtyNGCumulative,
    DateTime CreatedAt,
    string? CurrentOperatorID,
    DateTime? CurrentStartTime);

public record WIPByStyleDto(
    string StyleCode,
    string ColorCode,
    string SizeCode,
    string? CurrentOperationCode,
    string Status,
    int BundleCount,
    int TotalPieces);

public record LineBalancingDto(
    string OperationCode,
    int? WorkCenterID,
    int BundleCount,
    int TotalPieces);

public record OperatorEfficiencyDto(
    string OperatorID,
    string OperationCode,
    int MovementCount,
    decimal? AverageEfficiencyPct,
    decimal? AverageMinsPerPiece);

public interface IBundleRepository
{
    Task AddRangeAsync(IEnumerable<Bundle> bundles, CancellationToken ct = default);
    Task<Bundle?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<Bundle?> GetByIdAsync(int bundleId, CancellationToken ct = default);
    Task<BundleMovement?> GetOpenMovementAsync(int bundleId, CancellationToken ct = default);
    Task AddMovementAsync(BundleMovement movement, CancellationToken ct = default);
    Task<BundleLocationDto?> GetLocationAsync(string barcode, CancellationToken ct = default);
    Task<IReadOnlyList<WIPByStyleDto>> GetWIPByStyleAsync(
        string styleCode, string? colorCode, int? woid, CancellationToken ct = default);
    Task<IReadOnlyList<LineBalancingDto>> GetLineBalancingAsync(
        int? workCenterId, string? styleCode, CancellationToken ct = default);
    Task<IReadOnlyList<OperatorEfficiencyDto>> GetOperatorEfficiencyAsync(
        string? operatorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<int> GetConsecutiveLowEfficiencyCountAsync(
        string operatorId, string operationCode, decimal threshold, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
