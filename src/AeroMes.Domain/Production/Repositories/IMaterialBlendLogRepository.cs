namespace AeroMes.Domain.Production.Repositories;

public record MaterialBlendLogDto(
    long BlendLogID,
    long JobID,
    string ResinProductCode,
    string VirginLotNumber,
    decimal VirginQtyKg,
    string? RegrindLotNumber,
    decimal RegrindQtyKg,
    decimal TotalQtyKg,
    decimal RegrindRatioPct,
    decimal MaxAllowedPct,
    bool IsCompliant,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? ApprovalNotes,
    DateTime RecordedAt);

public record RegrindUsageSummaryDto(
    string ResinProductCode,
    decimal TotalVirginKg,
    decimal TotalRegrindKg,
    decimal TotalBlendedKg,
    decimal AverageRegrindRatioPct,
    int TotalBlends,
    int NonCompliantBlends,
    int ApprovedNonCompliantBlends);

public interface IMaterialBlendLogRepository
{
    Task AddAsync(MaterialBlendLog blendLog, CancellationToken ct = default);
    Task<MaterialBlendLog?> GetByIdAsync(long blendLogId, CancellationToken ct = default);
    Task<IReadOnlyList<MaterialBlendLogDto>> GetByJobAsync(long jobId, CancellationToken ct = default);
    Task<(IReadOnlyList<MaterialBlendLogDto> Items, int Total)> GetNonCompliantAsync(
        DateTime? fromDate, DateTime? toDate, bool? isApproved, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<RegrindUsageSummaryDto>> GetSummaryAsync(
        string? productCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
