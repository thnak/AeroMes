namespace AeroMes.Domain.Production.Repositories;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WorkOrder?> GetByCodeAsync(string woCode, CancellationToken ct = default);
    Task<IReadOnlyList<WorkOrder>> GetByStatusAsync(WorkOrderStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<WorkOrder>> GetByPoIdAsync(int poId, CancellationToken ct = default);
    Task AddAsync(WorkOrder entity, CancellationToken ct = default);
}
