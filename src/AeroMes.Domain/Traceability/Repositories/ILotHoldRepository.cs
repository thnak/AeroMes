namespace AeroMes.Domain.Traceability.Repositories;

public sealed record LotHoldDto(
    Guid HoldID,
    string LotNumber,
    string? ProductCode,
    int? WorkOrderID,
    string HoldStatus,
    string HoldReason,
    string? HoldDescription,
    string? HoldReference,
    string HoldInitiatedBy,
    DateTime HoldInitiatedAt,
    string? DispositionCode,
    string? DispositionNotes,
    string? ReleasedBy,
    DateTime? ReleasedAt,
    string? ESignatureRef);

public sealed record LotHoldStatusDto(
    string LotNumber,
    bool IsOnHold,
    Guid? ActiveHoldID,
    string? HoldReason,
    string? HoldReference,
    DateTime? HoldInitiatedAt);

public sealed record BulkHoldResultDto(
    int RequestedCount,
    int HoldsPlaced,
    IReadOnlyList<string> AffectedLots);

public interface ILotHoldRepository
{
    Task AddAsync(LotHold hold, CancellationToken ct = default);
    Task<LotHold?> GetByIdAsync(Guid holdId, CancellationToken ct = default);
    Task<bool> HasActiveHoldAsync(string lotNumber, CancellationToken ct = default);
    Task<LotHoldStatusDto> GetStatusAsync(string lotNumber, CancellationToken ct = default);
    Task<(IReadOnlyList<LotHoldDto> Items, int Total)> GetActiveHoldsAsync(string? lotNumber, string? holdReason, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<LotHoldDto>> GetHistoryAsync(string lotNumber, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
