namespace AeroMes.Domain.Master.Repositories;

public record FabricRollDto(
    int RollID,
    string RollBarcode,
    string FabricProductCode,
    string LotNumber,
    string ShadeCode,
    decimal GrossLengthMeters,
    decimal GrossWeightKg,
    decimal RemainingLengthMeters,
    decimal RemainingWeightKg,
    decimal FabricWidthCm,
    string? SupplierCode,
    DateOnly ReceivedDate,
    int? LocationID,
    string Status,
    DateTime CreatedAt);

public record FabricConsumptionLogDto(
    long ConsumptionID,
    int RollID,
    int CutOrderID,
    decimal MetersConsumed,
    decimal RemainingAfter,
    DateTime RecordedAt,
    string RecordedBy);

public record FabricInventorySummaryDto(
    string FabricProductCode,
    string ShadeCode,
    int TotalRolls,
    int AvailableRolls,
    decimal TotalMeters,
    decimal AvailableMeters,
    decimal TotalWeightKg,
    decimal AvailableWeightKg);

public interface IFabricRollRepository
{
    Task AddAsync(FabricRoll roll, CancellationToken ct = default);
    Task<FabricRoll?> GetByIdAsync(int rollId, CancellationToken ct = default);
    Task<FabricRoll?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<IReadOnlyList<FabricRoll>> GetByIdsAsync(IReadOnlyList<int> rollIds, CancellationToken ct = default);
    Task<IReadOnlyList<FabricRollDto>> GetAvailableByProductAndShadeAsync(
        string fabricProductCode, string? shadeCode, CancellationToken ct = default);
    Task<FabricRollDto?> GetDetailAsync(int rollId, CancellationToken ct = default);
    Task<IReadOnlyList<FabricConsumptionLogDto>> GetHistoryAsync(int rollId, CancellationToken ct = default);
    Task AddConsumptionLogAsync(AeroMes.Domain.Production.FabricConsumptionLog log, CancellationToken ct = default);
    Task<IReadOnlyList<FabricInventorySummaryDto>> GetInventorySummaryAsync(
        string? fabricProductCode, CancellationToken ct = default);
    Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
