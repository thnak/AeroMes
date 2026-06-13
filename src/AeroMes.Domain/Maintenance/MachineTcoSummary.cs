using AeroMes.Domain.Common;

namespace AeroMes.Domain.Maintenance;

public class MachineTcoSummary : Entity
{
    public int TcoID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public short PeriodYear { get; private set; }
    public byte PeriodMonth { get; private set; }
    public decimal PlannedMaintCost { get; private set; }
    public decimal ActualMaintCost { get; private set; }
    public int BreakdownCount { get; private set; }
    public decimal? MtbfHours { get; private set; }
    public decimal? MttrHours { get; private set; }
    public DateTime LastRefreshedAt { get; private set; }

    private MachineTcoSummary() { }

    public static MachineTcoSummary Create(
        string machineCode, short year, byte month,
        decimal plannedMaintCost, decimal actualMaintCost,
        int breakdownCount, decimal? mtbfHours, decimal? mttrHours)
        => new()
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            PeriodYear = year,
            PeriodMonth = month,
            PlannedMaintCost = plannedMaintCost,
            ActualMaintCost = actualMaintCost,
            BreakdownCount = breakdownCount,
            MtbfHours = mtbfHours,
            MttrHours = mttrHours,
            LastRefreshedAt = DateTime.UtcNow,
        };

    public void Refresh(
        decimal plannedMaintCost, decimal actualMaintCost,
        int breakdownCount, decimal? mtbfHours, decimal? mttrHours)
    {
        PlannedMaintCost = plannedMaintCost;
        ActualMaintCost = actualMaintCost;
        BreakdownCount = breakdownCount;
        MtbfHours = mtbfHours;
        MttrHours = mttrHours;
        LastRefreshedAt = DateTime.UtcNow;
    }
}
