namespace AeroMes.Domain.Maintenance.Repositories;

public record MaintCostLineDto(
    int LineID, int MaintOrderID, string CostCategory,
    string? ProductCode, decimal? QtyUsed, decimal? UnitCost,
    string? OperatorID, decimal? LaborHours,
    string? InvoiceRef, decimal? InvoiceAmount, decimal LineTotal,
    string PostedBy, DateTime PostedAt);

public record MaintenanceOrderDto(
    int MaintOrderID, string MaintOrderCode, string MachineCode,
    string OrderType, string? TriggerRef, string Status, string Priority,
    DateTime? PlannedStartAt, DateTime? PlannedEndAt,
    DateTime? ActualStartAt, DateTime? ActualEndAt,
    string? AssignedTo, decimal? EstimatedCost, decimal ActualTotalCost,
    string? Notes, DateTime CreatedAt,
    IReadOnlyList<MaintCostLineDto> Lines);

public interface IMaintenanceOrderRepository
{
    Task AddAsync(MaintenanceOrder order, CancellationToken ct);
    Task<MaintenanceOrder?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<(IReadOnlyList<MaintenanceOrderDto> Items, int Total)> GetListAsync(
        string? machineCode, MaintenanceOrderStatus? status,
        int page, int pageSize, CancellationToken ct);
}
