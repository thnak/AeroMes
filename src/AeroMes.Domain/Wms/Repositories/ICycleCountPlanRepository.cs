namespace AeroMes.Domain.Wms.Repositories;

public interface ICycleCountPlanRepository
{
    Task<IReadOnlyList<CycleCountPlan>> GetAllAsync(CycleCountPlanStatus? status = null, CancellationToken ct = default);
    Task<CycleCountPlan?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CycleCountPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> PlanCodeExistsAsync(string planCode, CancellationToken ct = default);
    Task AddAsync(CycleCountPlan plan, CancellationToken ct = default);
}
