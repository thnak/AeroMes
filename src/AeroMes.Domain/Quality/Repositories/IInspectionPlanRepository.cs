namespace AeroMes.Domain.Quality.Repositories;

public interface IInspectionPlanRepository
{
    Task<InspectionPlan?> GetByIdAsync(int planId, CancellationToken ct = default);
    Task<InspectionPlan?> GetByIdWithCharacteristicsAsync(int planId, CancellationToken ct = default);
    Task<InspectionPlan?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> HasLinkedInspectionOrdersAsync(int planId, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionPlan>> GetListAsync(
        int? routingStepId, string? productCode, bool? isActive, CancellationToken ct = default);
    void Add(InspectionPlan plan);
    void Remove(InspectionPlan plan);
}
