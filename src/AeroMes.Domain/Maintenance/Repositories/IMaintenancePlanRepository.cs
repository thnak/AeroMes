namespace AeroMes.Domain.Maintenance.Repositories;

public record PmTemplateDto(
    int TemplateId, string MachineCode, string PlanName,
    string TriggerType, int TriggerInterval, int EstimatedDurationMinutes,
    string Priority, bool IsActive, DateTime? LastGeneratedAt);

public record PmChecklistItemDto(int ItemId, int StepOrder, string Description,
    bool RequiresPartReplacement, string? PartCode);

public record MwoCalendarDto(
    int MwoId, int? TemplateId, string MachineCode, string TriggeredBy,
    string Priority, DateTime PlannedStartAt, DateTime? ActualStartAt,
    DateTime? ActualEndAt, string Status, string? AssignedTo, string? Notes);

public interface IMaintenancePlanRepository
{
    Task AddTemplateAsync(MaintenancePlanTemplate template, CancellationToken ct);
    Task<MaintenancePlanTemplate?> GetTemplateByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MaintenancePlanTemplate>> GetDueTemplatesAsync(DateTime asOf, CancellationToken ct);
    Task<IReadOnlyList<PmTemplateDto>> GetTemplateListAsync(string? machineCode, CancellationToken ct);
    Task AddWorkOrderAsync(MaintenanceWorkOrder mwo, CancellationToken ct);
    Task<MaintenanceWorkOrder?> GetWorkOrderByIdAsync(int id, CancellationToken ct);
    Task<bool> HasBlockingMwoAsync(string machineCode, CancellationToken ct);
    Task<IReadOnlyList<MwoCalendarDto>> GetCalendarAsync(DateTime from, DateTime to, string? machineCode, CancellationToken ct);
    Task AddChecklistResultAsync(MaintenanceChecklistResult result, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IMachineRuntimeRepository
{
    Task<MachineRuntimeAccumulator?> GetAsync(string machineCode, CancellationToken ct);
    Task UpsertAsync(MachineRuntimeAccumulator acc, CancellationToken ct);
}
