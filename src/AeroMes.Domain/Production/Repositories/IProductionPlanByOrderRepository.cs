namespace AeroMes.Domain.Production.Repositories;

public record ProductionPlanLineDto(
    int PlanLineId,
    string ProductCode,
    decimal PlannedQty,
    string? TeamCode,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    decimal ActualQty,
    bool IsLate);

public record ProductionPlanDto(
    int PlanId,
    string PlanCode,
    int PoId,
    string AllocationMethod,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    IReadOnlyList<ProductionPlanLineDto> Lines);

public record GanttIntervalDto(
    int PlanLineId,
    int PlanId,
    string PlanCode,
    string ProductCode,
    string Status,
    DateTime PlannedStart,
    DateTime PlannedEnd,
    decimal PlannedQty,
    decimal ActualQty,
    bool IsLate);

public record GanttTeamDto(
    string TeamCode,
    string TeamName,
    IReadOnlyList<GanttIntervalDto> Intervals);

public interface IProductionPlanByOrderRepository
{
    Task AddAsync(ProductionPlanByOrder plan, CancellationToken ct = default);
    Task<ProductionPlanByOrder?> GetByIdAsync(int planId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string planCode, CancellationToken ct = default);

    Task<(IReadOnlyList<ProductionPlanDto> Items, int Total)> GetListAsync(
        int? poId, ProductionPlanStatus? status,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<GanttTeamDto>> GetGanttDataAsync(
        DateTime fromDate, DateTime toDate,
        string? teamCode, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
