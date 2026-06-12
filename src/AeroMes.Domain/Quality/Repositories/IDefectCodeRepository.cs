namespace AeroMes.Domain.Quality.Repositories;

public interface IDefectCodeRepository
{
    Task<DefectCode?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DefectCode?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Dictionary<string, DefectCode>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct = default);
    Task<IReadOnlyList<DefectCode>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(DefectCode entity, CancellationToken ct = default);
}
