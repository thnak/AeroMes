using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class MachineProductConfig : Entity
{
    public string MachineCode { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int? RoutingStepId { get; private set; }
    public double IdealCycleTimeSeconds { get; private set; }
    public int TargetThroughputPerHour { get; private set; }
    public double SetupTimeSeconds { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }

    private MachineProductConfig() { }

    public static MachineProductConfig Create(
        string machineCode, string productCode,
        double idealCycleTimeSeconds, int targetThroughputPerHour,
        double setupTimeSeconds, DateOnly effectiveFrom,
        int? routingStepId = null)
    {
        return new MachineProductConfig
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            RoutingStepId = routingStepId,
            IdealCycleTimeSeconds = idealCycleTimeSeconds,
            TargetThroughputPerHour = targetThroughputPerHour,
            SetupTimeSeconds = setupTimeSeconds,
            EffectiveFrom = effectiveFrom
        };
    }

    public void Update(
        double idealCycleTimeSeconds, int targetThroughputPerHour,
        double setupTimeSeconds, DateOnly effectiveFrom)
    {
        IdealCycleTimeSeconds = idealCycleTimeSeconds;
        TargetThroughputPerHour = targetThroughputPerHour;
        SetupTimeSeconds = setupTimeSeconds;
        EffectiveFrom = effectiveFrom;
    }
}
