namespace AeroMes.Domain.Production.Repositories;

public record ScheduleListDto(
    int ScheduleId, string ScheduleName, string? FacilityCode,
    DateTime PeriodStart, DateTime PeriodEnd, string Status, int LineCount, DateTime CreatedAt);

public record ScheduleLineDto(
    int LineId, int POID, string POCode, string ProductCode, string? ProductName,
    int WorkCenterID, string WorkCenterCode, string WorkCenterName,
    DateTime PlannedStart, DateTime PlannedEnd, int SequenceNo, string? Notes);

public record ScheduleDetailDto(
    int ScheduleId, string ScheduleName, string? FacilityCode,
    DateTime PeriodStart, DateTime PeriodEnd, string Status, DateTime CreatedAt,
    IReadOnlyList<ScheduleLineDto> Lines);

public record PendingOrderDto(
    int POID, string POCode, string ProductCode, string? ProductName,
    int TargetQuantity, DateTime? PlannedStart, DateTime? PlannedEnd,
    DateTime? ProductionDeadline, byte Priority);

public record CapacityInfo(int WorkCenterID, double CycleTimeSeconds);

public interface IProductionScheduleRepository
{
    Task<int> AddAsync(ProductionSchedule schedule, CancellationToken ct);
    Task<ProductionSchedule?> GetByIdAsync(int id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task DeleteAsync(ProductionSchedule schedule, CancellationToken ct);
    Task<(IReadOnlyList<ScheduleListDto> Items, int Total)> GetListAsync(
        string? status, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct);
    Task<ScheduleDetailDto?> GetDetailAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PendingOrderDto>> GetPendingOrdersAsync(
        DateTime periodStart, DateTime periodEnd, int? excludeScheduleId, CancellationToken ct);
    Task<Dictionary<int, List<(DateTime Start, DateTime End)>>> GetScheduledSlotsByWorkCenterAsync(
        DateTime periodStart, DateTime periodEnd, int scheduleId, CancellationToken ct);
    Task<CapacityInfo?> GetPrimaryCapacityAsync(string productCode, CancellationToken ct);
}
