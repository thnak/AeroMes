using AeroMes.Domain.Common;

namespace AeroMes.Domain.Energy;

public class EnergyTarget : Entity
{
    public int TargetID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public decimal TargetKWhPerUnit { get; private set; }
    public decimal? TargetKWhPerShift { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    private EnergyTarget() { }

    public static EnergyTarget Create(
        string machineCode, decimal targetKWhPerUnit, decimal? targetKWhPerShift,
        DateOnly effectiveFrom, DateOnly? effectiveTo)
        => new()
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            TargetKWhPerUnit = targetKWhPerUnit,
            TargetKWhPerShift = targetKWhPerShift,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
        };
}
