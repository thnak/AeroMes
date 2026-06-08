namespace AeroMes.Domain.Production.Repositories;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<List<WorkOrder>> GetFilteredAsync(
        WorkOrderStatus? status,
        int? workCenterId,
        CancellationToken ct = default);

    Task AddAsync(WorkOrder workOrder, CancellationToken ct = default);
}
