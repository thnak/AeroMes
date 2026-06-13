namespace AeroMes.Domain.Quality.Repositories;

public record InlineInspectionDto(
    long InspectionID,
    int WOID,
    int WorkCenterID,
    string StyleCode,
    string? ColorCode,
    string InspectorID,
    string ShiftCode,
    int SampleSize,
    int TotalDefects,
    decimal DHU,
    decimal DHU_Target,
    bool IsAboveTarget,
    DateTime InspectedAt,
    string? Notes);

public record DHUTrendDto(
    DateTime Period,
    string StyleCode,
    int? WorkCenterID,
    string? ShiftCode,
    decimal AvgDHU,
    int InspectionCount,
    int AboveTargetCount);

public record DefectParetoDto(
    string DefectCode,
    string? DefectCategory,
    bool IsMajor,
    int TotalQuantity,
    int OccurrenceCount,
    decimal FrequencyPct);

public record QualitySummaryByStyleDto(
    string StyleCode,
    decimal AvgDHU,
    int InlineInspectionCount,
    int InlineAboveTargetCount,
    int AQLInspectionCount,
    int AQLAcceptCount,
    int AQLRejectCount,
    IReadOnlyList<DefectParetoDto> TopDefects);

public interface IInlineInspectionRepository
{
    Task AddAsync(InlineInspection inspection, CancellationToken ct = default);
    Task<IReadOnlyList<DHUTrendDto>> GetDHUTrendAsync(int? woid, int? workCenterId, string? styleCode, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<IReadOnlyList<DefectParetoDto>> GetDefectParetoAsync(string? styleCode, int? workCenterId, DateTime fromDate, DateTime toDate, int topN, CancellationToken ct = default);
    Task<QualitySummaryByStyleDto?> GetQualitySummaryAsync(string styleCode, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
