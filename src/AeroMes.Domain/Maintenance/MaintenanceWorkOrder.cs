using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Maintenance;

public enum MwoStatus { Scheduled, InProgress, Completed, Cancelled }
public enum MwoTriggeredBy { Schedule, Manual, Breakdown }

public class MaintenanceWorkOrder : Entity
{
    public int MwoId { get; private set; }
    public int? TemplateId { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public MwoTriggeredBy TriggeredBy { get; private set; }
    public string? AssignedTo { get; private set; }
    public MaintenancePriority Priority { get; private set; }
    public DateTime PlannedStartAt { get; private set; }
    public DateTime? ActualStartAt { get; private set; }
    public DateTime? ActualEndAt { get; private set; }
    public MwoStatus Status { get; private set; } = MwoStatus.Scheduled;
    public string? Notes { get; private set; }

    public MaintenancePlanTemplate? Template { get; private set; }
    private readonly List<MaintenanceChecklistResult> _results = [];
    public IReadOnlyList<MaintenanceChecklistResult> Results => _results.AsReadOnly();

    private MaintenanceWorkOrder() { }

    public static MaintenanceWorkOrder Create(
        int? templateId, string machineCode, MwoTriggeredBy triggeredBy,
        MaintenancePriority priority, DateTime plannedStartAt,
        string? assignedTo = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(machineCode)) throw new DomainException("Mã máy không được để trống.");

        return new MaintenanceWorkOrder
        {
            TemplateId = templateId,
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            TriggeredBy = triggeredBy,
            Priority = priority,
            PlannedStartAt = plannedStartAt,
            AssignedTo = assignedTo?.Trim(),
            Notes = notes?.Trim(),
            Status = MwoStatus.Scheduled,
        };
    }

    public void Start()
    {
        if (Status != MwoStatus.Scheduled)
            throw new DomainException($"Lệnh bảo trì phòng ngừa phải ở trạng thái Đã lập lịch. Hiện tại: {Status}.");
        Status = MwoStatus.InProgress;
        ActualStartAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != MwoStatus.InProgress)
            throw new DomainException($"Lệnh bảo trì phòng ngừa phải đang thực hiện để hoàn thành. Hiện tại: {Status}.");
        Status = MwoStatus.Completed;
        ActualEndAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is MwoStatus.Completed or MwoStatus.Cancelled)
            throw new DomainException($"Không thể hủy lệnh bảo trì ở trạng thái {Status}.");
        Status = MwoStatus.Cancelled;
    }

    public bool BlocksProduction()
        => Status == MwoStatus.Scheduled
           && Priority is MaintenancePriority.High or MaintenancePriority.Critical
           && PlannedStartAt <= DateTime.UtcNow;
}
