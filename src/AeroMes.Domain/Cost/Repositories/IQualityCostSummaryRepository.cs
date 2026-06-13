namespace AeroMes.Domain.Cost.Repositories;

public record CopqTrendPointDto(short Year, byte Month, string? ProductCode, decimal TotalQualityCost, decimal? CopqPct);
public record QualityCostSummaryDto(
    int SummaryID, short PeriodYear, byte PeriodMonth, string? ProductCode, int? WorkCenterID,
    decimal PreventionCost, decimal AppraisalCost, decimal InternalScrapCost, decimal ReworkCost,
    decimal CustomerReturnCost, decimal WarrantyCost, decimal TotalQualityCost,
    decimal? TotalProductionValue, decimal? CopqPct, DateTime LastRefreshedAt);

public interface IQualityCostSummaryRepository
{
    Task<QualityCostSummary?> GetByPeriodAsync(short year, byte month, string? productCode, int? workCenterId, CancellationToken ct);
    Task AddAsync(QualityCostSummary summary, CancellationToken ct);
    Task<IReadOnlyList<QualityCostSummaryDto>> GetSummaryAsync(short year, byte? month, string? productCode, CancellationToken ct);
    Task<IReadOnlyList<CopqTrendPointDto>> GetCopqTrendAsync(int months, string? productCode, CancellationToken ct);
}
