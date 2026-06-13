namespace AeroMes.Domain.Quality.Repositories;

public record QualityCriteriaGroupDto(int GroupID, string Code, string Name, string Status, DateTime CreatedAt);

public interface IQualityCriteriaGroupRepository
{
    Task<int> AddAsync(QualityCriteriaGroup group, CancellationToken ct);
    Task<QualityCriteriaGroup?> GetByIdAsync(int groupId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> HasCriteriaReferencesAsync(int groupId, CancellationToken ct);
    Task<IReadOnlyList<QualityCriteriaGroupDto>> GetListAsync(string? keyword, bool includeInactive, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task DeleteAsync(QualityCriteriaGroup group, CancellationToken ct);
}
