using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Machine : AuditableEntity
{
    public string MachineCode { get; private set; } = string.Empty;  // PK — e.g. MCH-CNC-01
    public string MachineName { get; private set; } = string.Empty;
    public int WorkCenterID { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public MachineStatus Status { get; private set; } = MachineStatus.Offline;
    public bool IsActive { get; private set; } = true;

    // EF navigation
    public WorkCenter? WorkCenter { get; private set; }

    private Machine() { }

    public static Machine Create(
        string code,
        string name,
        int workCenterId,
        string? brand = null,
        string? model = null,
        string? createdBy = null)
    {
        return new Machine
        {
            MachineCode = code.Trim().ToUpperInvariant(),
            MachineName = name.Trim(),
            WorkCenterID = workCenterId,
            Brand = brand,
            Model = model,
            Status = MachineStatus.Offline,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, int workCenterId, string? brand, string? model, string updatedBy)
    {
        MachineName = name.Trim();
        WorkCenterID = workCenterId;
        Brand = brand;
        Model = model;
        Touch(updatedBy);
    }

    public void SetStatus(MachineStatus status) => Status = status;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public enum MachineStatus { Running, Down, Idle, Offline }
