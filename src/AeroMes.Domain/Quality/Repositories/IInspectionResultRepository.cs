namespace AeroMes.Domain.Quality.Repositories;

public interface IInspectionResultRepository
{
    Task<IReadOnlyList<InspectionResult>> GetByOrderAsync(int inspectionOrderId, CancellationToken ct = default);
    Task<int> CountByOrderAsync(int inspectionOrderId, CancellationToken ct = default);
    Task<int> CountFailedByOrderAsync(int inspectionOrderId, CancellationToken ct = default);
    Task<bool> HasResultForCharAsync(int inspectionOrderId, int charId, CancellationToken ct = default);
    void Add(InspectionResult result);
    void AddRange(IEnumerable<InspectionResult> results);
}
