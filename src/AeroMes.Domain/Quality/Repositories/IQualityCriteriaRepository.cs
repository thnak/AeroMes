namespace AeroMes.Domain.Quality.Repositories;

public record QualityCriteriaDto(
    int CriteriaID, string Code, string Name,
    int? GroupID, string? GroupName,
    string CriteriaType, string? InspectionMethod, string? MethodDescription,
    string Status, DateTime CreatedAt);

public interface IQualityCriteriaRepository
{
    Task<int> AddAsync(QualityCriteria criteria, CancellationToken ct);
    Task<QualityCriteria?> GetByIdAsync(int criteriaId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> HasInspectionReferencesAsync(int criteriaId, CancellationToken ct);
    Task<IReadOnlyList<QualityCriteriaDto>> GetListAsync(
        string? keyword, string? status, int? groupId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task DeleteAsync(QualityCriteria criteria, CancellationToken ct);
}
