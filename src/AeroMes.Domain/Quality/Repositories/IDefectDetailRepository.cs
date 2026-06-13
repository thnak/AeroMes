namespace AeroMes.Domain.Quality.Repositories;

public interface IDefectDetailRepository
{
    Task<IReadOnlyList<DefectDetail>> GetForReportAsync(
        DateTime from, DateTime to, string? defectCategory, CancellationToken ct = default);
}
