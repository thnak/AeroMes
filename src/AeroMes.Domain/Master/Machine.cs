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

    // Capacity & OEE baseline
    public string? MachineCategory { get; private set; }
    public decimal? TargetOeePct { get; private set; }
    public decimal? TheoreticalCapacityPerHour { get; private set; }
    public int PlannedDowntimeMinPerShift { get; private set; }

    // Cost
    public decimal? HourlyCostRate { get; private set; }

    // Industry type + technical spec
    public string MachineType { get; private set; } = MachineTypes.General;
    public string? CustomAttributes { get; private set; }

    // Computed from CustomAttributes (populated by EF Core, not set directly)
    public int? ClampingForceTons { get; private set; }
    public string? SewingMachineClass { get; private set; }

    // IoT integration hook
    public string? OpcUaNodeId { get; private set; }

    // Operator certification requirements
    public bool RequiresCertification { get; private set; }
    public string? CertificationCode { get; private set; }
    public byte MaxOperators { get; private set; } = 1;

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

    public void UpdateCapacity(
        string? machineCategory,
        decimal? targetOeePct,
        decimal? theoreticalCapacityPerHour,
        int plannedDowntimeMinPerShift,
        decimal? hourlyCostRate,
        string? opcUaNodeId,
        bool requiresCertification,
        string? certificationCode,
        byte maxOperators,
        string updatedBy)
    {
        MachineCategory = machineCategory;
        TargetOeePct = targetOeePct;
        TheoreticalCapacityPerHour = theoreticalCapacityPerHour;
        PlannedDowntimeMinPerShift = plannedDowntimeMinPerShift;
        HourlyCostRate = hourlyCostRate;
        OpcUaNodeId = opcUaNodeId;
        RequiresCertification = requiresCertification;
        CertificationCode = certificationCode;
        MaxOperators = maxOperators;
        Touch(updatedBy);
    }

    public void SetMachineType(string machineType, string updatedBy)
    {
        MachineType = machineType;
        Touch(updatedBy);
    }

    public void SetCustomAttributes(string? customAttributesJson, string updatedBy)
    {
        CustomAttributes = customAttributesJson;
        Touch(updatedBy);
    }

    public void SetStatus(MachineStatus status) => Status = status;

    public void Activate(string updatedBy)
    {
        IsActive = true;
        Touch(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        Touch(updatedBy);
    }
}

public enum MachineStatus { Running, Down, Idle, Offline }

public static class MachineTypes
{
    public const string General         = "GENERAL";
    public const string InjectionMold   = "INJECTION_MOLD";
    public const string BlowMold        = "BLOW_MOLD";
    public const string Extrusion       = "EXTRUSION";
    public const string Thermoform      = "THERMOFORM";
    public const string OvenDryer       = "OVEN_DRYER";
    public const string Sewing          = "SEWING";
    public const string CuttingMachine  = "CUTTING_MACHINE";
    public const string Embroidery      = "EMBROIDERY";
    public const string Pressing        = "PRESSING";
    public const string Cnc             = "CNC";
    public const string Assembly        = "ASSEMBLY";
    public const string Packaging       = "PACKAGING";

    public static readonly IReadOnlyList<string> All =
    [
        General, InjectionMold, BlowMold, Extrusion, Thermoform, OvenDryer,
        Sewing, CuttingMachine, Embroidery, Pressing, Cnc, Assembly, Packaging,
    ];
}
