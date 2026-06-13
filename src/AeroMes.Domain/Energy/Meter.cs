using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Energy;

public enum UtilityType { Electricity, CompressedAir, Water, Gas, ChilledWater }

public class Meter : AuditableEntity
{
    public int MeterID { get; private set; }
    public string MeterCode { get; private set; } = string.Empty;
    public string MeterName { get; private set; } = string.Empty;
    public UtilityType UtilityType { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? MachineCode { get; private set; }
    public int? WorkCenterID { get; private set; }
    public bool IsSubMeter { get; private set; }
    public int? ParentMeterID { get; private set; }
    public int? TariffID { get; private set; }
    public string? OpcUaNodeId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Meter() { }

    public static Meter Create(
        string code, string name, UtilityType utilityType, string unit,
        string? machineCode, int? workCenterId, bool isSubMeter, int? parentMeterId,
        int? tariffId, string? opcUaNodeId, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã đồng hồ không được để trống.");
        return new Meter
        {
            MeterCode = code.Trim().ToUpperInvariant(),
            MeterName = name.Trim(),
            UtilityType = utilityType,
            Unit = unit.Trim(),
            MachineCode = machineCode?.Trim().ToUpperInvariant(),
            WorkCenterID = workCenterId,
            IsSubMeter = isSubMeter,
            ParentMeterID = parentMeterId,
            TariffID = tariffId,
            OpcUaNodeId = opcUaNodeId?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Deactivate(string? updatedBy)
    {
        IsActive = false;
        Touch(updatedBy);
    }
}
