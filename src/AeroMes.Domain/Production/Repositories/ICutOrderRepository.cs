namespace AeroMes.Domain.Production.Repositories;

public record CutOrderLineDto(
    int LineID, string SizeCode, int QuantityToCut, int QuantityCut);

public record CutOrderFabricUsageDto(
    int UsageID, int RollID, decimal MetersUsed);

public record CutOrderDto(
    int CutOrderID,
    string CutOrderCode,
    int WOID,
    string StyleCode,
    string ColorCode,
    string FabricProductCode,
    string ShadeCode,
    string? MarkerReference,
    decimal? MarkerEfficiencyPct,
    int PlyCount,
    decimal SpreadLengthMeters,
    decimal FabricWidthCm,
    decimal? EstimatedFabricMeters,
    decimal? ActualFabricMeters,
    decimal? FabricWastePct,
    string Status,
    DateTime? CuttingStartedAt,
    DateTime? CuttingCompletedAt,
    DateTime CreatedAt,
    IReadOnlyList<CutOrderLineDto> Lines,
    IReadOnlyList<CutOrderFabricUsageDto> FabricUsage);

public record MarkerEfficiencyReportDto(
    string StyleCode,
    string ColorCode,
    int CutOrderID,
    string CutOrderCode,
    decimal? MarkerEfficiencyPct,
    decimal? FabricWastePct,
    DateTime? CuttingCompletedAt);

public interface ICutOrderRepository
{
    Task AddAsync(CutOrder cutOrder, CancellationToken ct = default);
    Task<CutOrder?> GetByIdAsync(int cutOrderId, CancellationToken ct = default);
    Task<CutOrderDto?> GetDetailAsync(int cutOrderId, CancellationToken ct = default);
    Task<IReadOnlyList<CutOrderDto>> GetByWOAsync(int woid, CancellationToken ct = default);
    Task<IReadOnlyList<MarkerEfficiencyReportDto>> GetMarkerEfficiencyReportAsync(
        DateTime? fromDate, DateTime? toDate, string? styleCode, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string cutOrderCode, CancellationToken ct = default);
    Task AddBundlesAsync(IEnumerable<Bundle> bundles, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
