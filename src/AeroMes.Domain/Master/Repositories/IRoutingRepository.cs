namespace AeroMes.Domain.Master.Repositories;

public interface IRoutingRepository
{
    Task<Routing?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Routing?> GetByIdWithStepsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Routing>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<RoutingStep?> GetStepByIdAsync(int routingStepId, CancellationToken ct = default);
    Task AddAsync(Routing entity, CancellationToken ct = default);
    void RemoveSteps(IEnumerable<RoutingStep> steps);
    void AddStep(RoutingStep step);
}
