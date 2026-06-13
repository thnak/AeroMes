using AeroMes.Domain.Common;

namespace AeroMes.Domain.Cost;

public class StandardCostRoutingLine : Entity
{
    public int LineId { get; private set; }
    public int StdCostId { get; private set; }
    public int RoutingStepId { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public decimal CycleTimeSec { get; private set; }
    public decimal LaborRateSnapshot { get; private set; }
    public decimal MachineRateSnapshot { get; private set; }
    public decimal LaborCostLine => CycleTimeSec / 3600m * LaborRateSnapshot;
    public decimal MachineCostLine => CycleTimeSec / 3600m * MachineRateSnapshot;

    private StandardCostRoutingLine() { }

    public static StandardCostRoutingLine Create(
        int stdCostId, int routingStepId, string stepName,
        decimal cycleTimeSec, decimal laborRate, decimal machineRate)
        => new()
        {
            StdCostId = stdCostId,
            RoutingStepId = routingStepId,
            StepName = stepName.Trim(),
            CycleTimeSec = cycleTimeSec,
            LaborRateSnapshot = laborRate,
            MachineRateSnapshot = machineRate,
        };
}
