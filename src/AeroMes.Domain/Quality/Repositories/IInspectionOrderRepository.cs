namespace AeroMes.Domain.Quality.Repositories;

public interface IInspectionOrderRepository
{
    Task<InspectionOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InspectionOrder?> GetByIdWithPlanAsync(int id, CancellationToken ct = default);
    Task<InspectionOrder?> GetByJobIdAsync(long jobId, CancellationToken ct = default);
    Task<bool> HasOpenOrderForJobAsync(long jobId, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionOrder>> GetPendingAsync(string? workCenter, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionOrder>> GetFilteredAsync(string? status, DateOnly? date, CancellationToken ct = default);
    void Add(InspectionOrder order);
}
