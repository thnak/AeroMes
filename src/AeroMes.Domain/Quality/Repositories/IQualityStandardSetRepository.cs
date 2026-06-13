using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality.Repositories;

public record StandardSetListDto(
    int StandardSetID, string Code, string Name,
    string ProductCode, string? ProductName,
    int SamplingMethodID, string SamplingMethodName,
    DateOnly EffectiveDate, string Status, DateTime CreatedAt);

public record StandardSetCriteriaDto(int ID, int CriteriaID, string CriteriaCode, string CriteriaName, string? Parameters);

public record StandardSetStageCriteriaDto(
    int ID, int ProductionStageID, int SortOrder,
    int CriteriaID, string CriteriaCode, string CriteriaName,
    int? SamplingMethodID, string? SamplingMethodName, string? Parameters);

public record StandardSetDetailDto(
    int StandardSetID, string Code, string Name,
    string ProductCode, string? ProductName,
    int? ProductionProcessId, string? ProductionProcessName,
    int SamplingMethodID, string SamplingMethodName,
    DateOnly EffectiveDate, string? Notes, string Status,
    IReadOnlyList<StandardSetCriteriaDto> ProductCriteria,
    IReadOnlyList<StandardSetStageCriteriaDto> StageCriteria,
    DateTime CreatedAt);

public interface IQualityStandardSetRepository
{
    Task<int> AddAsync(QualityStandardSet standardSet, CancellationToken ct = default);
    Task<QualityStandardSet?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<bool> HasInspectionReferencesAsync(int standardSetId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task DeleteAsync(QualityStandardSet standardSet, CancellationToken ct = default);

    Task<(IReadOnlyList<StandardSetListDto> Items, int Total)> GetListAsync(
        string? keyword, string? productCode, string? status, int page, int pageSize, CancellationToken ct = default);

    Task<StandardSetDetailDto?> GetDetailAsync(int id, CancellationToken ct = default);

    Task<StandardSetDetailDto?> GetEffectiveAsync(string productCode, DateOnly date, CancellationToken ct = default);
}
