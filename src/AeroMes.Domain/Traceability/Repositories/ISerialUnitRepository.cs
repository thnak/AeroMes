namespace AeroMes.Domain.Traceability.Repositories;

public record SerialUnitDto(
    Guid SerialID,
    string SerialNumber,
    string? GTIN,
    string LotNumber,
    string ProductCode,
    int? WorkOrderID,
    DateOnly ProductionDate,
    DateOnly? ExpiryDate,
    string Status,
    string? UDI,
    DateTime CreatedAt);

public record SerialUnitDetailDto(
    Guid SerialID,
    string SerialNumber,
    string? GTIN,
    string LotNumber,
    string ProductCode,
    int? WorkOrderID,
    DateOnly ProductionDate,
    DateOnly? ExpiryDate,
    string Status,
    string? UDI,
    DateTime CreatedAt,
    IReadOnlyList<SerialLotLineageDto> ComponentLots,
    string? CurrentCaseSSCC,
    string? CurrentPalletSSCC);

public record SerialLotLineageDto(
    long ID,
    Guid SerialID,
    string ComponentLotNumber,
    string ComponentProductCode,
    decimal? QuantityUsed,
    string? UoM,
    int? RoutingStepID,
    DateTime AssembledAt);

public record SerialEventDto(
    long EventID,
    string EventType,
    Guid SerialID,
    int? WorkOrderID,
    string? LocationCode,
    decimal? Quantity,
    string? Payload,
    string? OperatorCode,
    DateTime EventTimestamp,
    DateTime RecordedAt);

public record SSCCContentsDto(
    string SSCC,
    IReadOnlyList<string> SerialNumbers,
    IReadOnlyList<string> ChildSSCCs,
    int TotalUnits,
    bool IsActive);

public interface ISerialUnitRepository
{
    Task AddAsync(SerialUnit unit, CancellationToken ct = default);
    Task AddLineageAsync(SerialLotLineage lineage, CancellationToken ct = default);
    Task AddAggregationAsync(SerialAggregation aggregation, CancellationToken ct = default);
    Task AddEventAsync(SerialEvent serialEvent, CancellationToken ct = default);

    Task<SerialUnit?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SerialUnit>> GetBySerialNumbersAsync(IReadOnlyList<string> serialNumbers, CancellationToken ct = default);

    Task<SerialUnitDetailDto?> GetDetailAsync(string serialNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SerialEventDto>> GetTimelineAsync(string serialNumber, CancellationToken ct = default);
    Task<IReadOnlyList<SerialLotLineageDto>> GetComponentLotsAsync(string serialNumber, CancellationToken ct = default);

    Task<(IReadOnlyList<SerialUnitDto> Items, int Total)> GetByLotAsync(
        string lotNumber, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<SerialUnitDto>> GetByComponentLotAsync(
        string componentLotNumber, CancellationToken ct = default);

    Task<SSCCContentsDto> GetSSCCContentsAsync(string sscc, CancellationToken ct = default);

    Task<IReadOnlyList<SerialAggregation>> GetActiveAggregationsBySSCCAsync(
        string sscc, CancellationToken ct = default);

    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct = default);
    Task<int> GetSerialCountForLotAsync(string lotNumber, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
