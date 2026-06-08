namespace AeroMes.Domain.Quality.Repositories;

public interface IDefectCodeRepository
{
    Task<DefectCode?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<Dictionary<string, DefectCode>> GetByCodesAsync(
        IEnumerable<string> codes,
        CancellationToken ct = default);
}
