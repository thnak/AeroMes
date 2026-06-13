namespace AeroMes.Domain.Traceability.Repositories;

public sealed record RecallSummaryDto(
    Guid RecallID,
    string RecallCode,
    string Title,
    string RecallType,
    string Status,
    string AnchorLotNumber,
    string InitiatedBy,
    DateTime InitiatedAt,
    DateTime? ScopeIdentifiedAt,
    DateTime? ClosedAt);

public sealed record RecallScopeLotDto(
    long RecallScopeLotID,
    Guid RecallID,
    string LotNumber,
    string? ProductCode,
    int TraceDepth,
    string LotCategory,
    string? CurrentLocationCode,
    decimal? QtyOnHand,
    string? ShipmentRef,
    string? CustomerRef,
    Guid? HoldID,
    DateTime AddedAt);

public sealed record RecallScopeDto(
    Guid RecallID,
    string AnchorLotNumber,
    int TotalAffectedLots,
    int InProcessWIPCount,
    int InWarehouseCount,
    int ShippedCount,
    IReadOnlyList<RecallScopeLotDto> Lots,
    long ScopeQueryMs);

public sealed record RecallAuditEntryDto(
    long AuditID,
    Guid RecallID,
    string ActionType,
    string? ActionDetail,
    string PerformedBy,
    DateTime PerformedAt,
    bool SystemGenerated);

public sealed record RecallDetailDto(
    Guid RecallID,
    string RecallCode,
    string Title,
    string RecallType,
    string Status,
    string AnchorLotNumber,
    string AnchorDirection,
    string? Description,
    string? RegulatoryRef,
    string InitiatedBy,
    DateTime InitiatedAt,
    DateTime? ScopeIdentifiedAt,
    DateTime? ClosedAt,
    string? ClosedBy,
    int ScopeLotCount);

public sealed record RecallAuditReportDto(
    RecallDetailDto Recall,
    RecallScopeDto Scope,
    IReadOnlyList<RecallAuditEntryDto> Timeline,
    IReadOnlyList<LotHoldDto> HoldDispositions);

public sealed record RecallQuarantineResultDto(
    Guid RecallID,
    int HoldsPlaced,
    IReadOnlyList<string> AffectedLots);

public interface IRecallRepository
{
    Task AddAsync(Recall recall, CancellationToken ct = default);
    Task AddScopeLotAsync(RecallScopeLot scopeLot, CancellationToken ct = default);
    Task AddAuditEntryAsync(RecallAuditEntry entry, CancellationToken ct = default);
    Task<Recall?> GetByIdAsync(Guid recallId, CancellationToken ct = default);
    Task<RecallDetailDto?> GetDetailAsync(Guid recallId, CancellationToken ct = default);
    Task<RecallScopeDto> GetScopeAsync(Guid recallId, CancellationToken ct = default);
    Task<IReadOnlyList<RecallAuditEntryDto>> GetAuditLogAsync(Guid recallId, CancellationToken ct = default);
    Task<(IReadOnlyList<RecallSummaryDto> Items, int Total)> ListAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<bool> HasUnresolvedHoldsAsync(Guid recallId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RecallScopeLot>> GetScopeLotsAsync(Guid recallId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
