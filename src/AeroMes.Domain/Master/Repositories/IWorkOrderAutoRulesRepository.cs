namespace AeroMes.Domain.Master.Repositories;

public interface IWorkOrderAutoRulesRepository
{
    Task<WorkOrderAutoRules?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WorkOrderAutoRules?> GetFactoryWideAsync(CancellationToken ct = default);
    Task<WorkOrderAutoRules?> GetByWorkCenterAsync(int workCenterId, CancellationToken ct = default);
    Task<IReadOnlyList<WorkOrderAutoRules>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(WorkOrderAutoRules entity, CancellationToken ct = default);
}
