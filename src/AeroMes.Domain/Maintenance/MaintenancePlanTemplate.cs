using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Maintenance;

public enum PmTriggerType { Calendar, RuntimeHours, CycleCount }

public class MaintenancePlanTemplate : AuditableEntity
{
    public int TemplateId { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public PmTriggerType TriggerType { get; private set; }
    public int TriggerInterval { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public MaintenancePriority Priority { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastGeneratedAt { get; private set; }

    private readonly List<MaintenanceChecklistItem> _items = [];
    public IReadOnlyList<MaintenanceChecklistItem> Items => _items.AsReadOnly();

    private MaintenancePlanTemplate() { }

    public static MaintenancePlanTemplate Create(
        string machineCode, string planName, PmTriggerType triggerType,
        int triggerInterval, int estimatedDurationMinutes,
        MaintenancePriority priority, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(machineCode)) throw new DomainException("Mã máy không được để trống.");
        if (string.IsNullOrWhiteSpace(planName)) throw new DomainException("Tên kế hoạch không được để trống.");
        if (triggerInterval <= 0) throw new DomainException("Chu kỳ bảo trì phải lớn hơn 0.");

        return new MaintenancePlanTemplate
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            PlanName = planName.Trim(),
            TriggerType = triggerType,
            TriggerInterval = triggerInterval,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            Priority = priority,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkGenerated(DateTime at) => LastGeneratedAt = at;

    public void Deactivate(string updatedBy) { IsActive = false; Touch(updatedBy); }
    public void Activate(string updatedBy) { IsActive = true; Touch(updatedBy); }

    public bool IsDueForCalendar(DateTime now)
        => TriggerType == PmTriggerType.Calendar
           && (LastGeneratedAt is null || (now - LastGeneratedAt.Value).TotalDays >= TriggerInterval);
}
