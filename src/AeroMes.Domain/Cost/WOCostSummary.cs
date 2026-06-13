using AeroMes.Domain.Common;
using AeroMes.Domain.Cost.Events;

namespace AeroMes.Domain.Cost;

public class WOCostSummary : Entity
{
    public int WOCostID { get; private set; }
    public int WOID { get; private set; }
    public int? StdCostID { get; private set; }
    public decimal StdTotalCost { get; private set; }
    public decimal ActMaterialCost { get; private set; }
    public decimal ActLaborCost { get; private set; }
    public decimal ActMachineCost { get; private set; }
    public decimal ActMaintenanceCost { get; private set; }
    public decimal ActTotalCost => ActMaterialCost + ActLaborCost + ActMachineCost + ActMaintenanceCost;
    public decimal TotalVariance => ActTotalCost - StdTotalCost;
    public int ProducedQtyOK { get; private set; }
    public int ScrapQty { get; private set; }
    public string? VarianceDetailJson { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private WOCostSummary() { }

    public static WOCostSummary Create(int woId, int? stdCostId, decimal stdTotalCost)
        => new()
        {
            WOID = woId, StdCostID = stdCostId, StdTotalCost = stdTotalCost,
            UpdatedAt = DateTime.UtcNow
        };

    public void AddMaterialCost(decimal amount)
    {
        ActMaterialCost += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLaborCost(decimal amount)
    {
        ActLaborCost += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMachineCost(decimal amount)
    {
        ActMachineCost += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMaintenanceCost(decimal amount)
    {
        ActMaintenanceCost += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordOutput(int qtyOk, int scrapQty)
    {
        ProducedQtyOK += qtyOk;
        ScrapQty += scrapQty;
        UpdatedAt = DateTime.UtcNow;
    }

    public WOCostOverrunEvent? SetVarianceDetail(string varianceJson, decimal overrunThresholdPct = 10m)
    {
        VarianceDetailJson = varianceJson;
        UpdatedAt = DateTime.UtcNow;

        if (StdTotalCost > 0 && TotalVariance / StdTotalCost * 100m > overrunThresholdPct)
            return new WOCostOverrunEvent(WOID, StdTotalCost, ActTotalCost, TotalVariance / StdTotalCost * 100m);

        return null;
    }
}

public class WOMaterialCostLine
{
    public long LineID { get; private set; }
    public int WOID { get; private set; }
    public long? ConsumptionID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? LotNumber { get; private set; }
    public decimal QtyConsumed { get; private set; }
    public decimal ActualUnitCost { get; private set; }
    public decimal LineTotal => QtyConsumed * ActualUnitCost;
    public DateTime PostedAt { get; private set; } = DateTime.UtcNow;

    private WOMaterialCostLine() { }

    public static WOMaterialCostLine Create(
        int woId, long? consumptionId, string productCode,
        string? lotNumber, decimal qtyConsumed, decimal actualUnitCost)
        => new()
        {
            WOID = woId, ConsumptionID = consumptionId, ProductCode = productCode.Trim(),
            LotNumber = lotNumber?.Trim(), QtyConsumed = qtyConsumed,
            ActualUnitCost = actualUnitCost, PostedAt = DateTime.UtcNow
        };
}

public class WOLaborCostLine
{
    public long LineID { get; private set; }
    public int WOID { get; private set; }
    public long JobID { get; private set; }
    public string OperatorID { get; private set; } = string.Empty;
    public int LaborGradeID { get; private set; }
    public decimal ActualHours { get; private set; }
    public decimal HourlyRateSnapshot { get; private set; }
    public bool IsOvertime { get; private set; }
    public decimal OvertimeMultiplierSnapshot { get; private set; } = 1.5m;
    public decimal LineTotal => ActualHours * HourlyRateSnapshot *
        (1m + (IsOvertime ? OvertimeMultiplierSnapshot - 1m : 0m));
    public DateTime PostedAt { get; private set; } = DateTime.UtcNow;

    private WOLaborCostLine() { }

    public static WOLaborCostLine Create(
        int woId, long jobId, string operatorId, int laborGradeId,
        decimal actualHours, decimal hourlyRateSnapshot,
        bool isOvertime, decimal overtimeMultiplierSnapshot)
        => new()
        {
            WOID = woId, JobID = jobId, OperatorID = operatorId.Trim(),
            LaborGradeID = laborGradeId, ActualHours = actualHours,
            HourlyRateSnapshot = hourlyRateSnapshot, IsOvertime = isOvertime,
            OvertimeMultiplierSnapshot = overtimeMultiplierSnapshot,
            PostedAt = DateTime.UtcNow
        };
}

public class WOMachineCostLine
{
    public long LineID { get; private set; }
    public int WOID { get; private set; }
    public long JobID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public decimal RuntimeHours { get; private set; }
    public decimal? EnergyKWh { get; private set; }
    public decimal TotalRateSnapshot { get; private set; }
    public decimal LineTotal => RuntimeHours * TotalRateSnapshot;
    public DateTime PostedAt { get; private set; } = DateTime.UtcNow;

    private WOMachineCostLine() { }

    public static WOMachineCostLine Create(
        int woId, long jobId, string machineCode,
        decimal runtimeHours, decimal? energyKWh, decimal totalRateSnapshot)
        => new()
        {
            WOID = woId, JobID = jobId, MachineCode = machineCode.Trim(),
            RuntimeHours = runtimeHours, EnergyKWh = energyKWh,
            TotalRateSnapshot = totalRateSnapshot, PostedAt = DateTime.UtcNow
        };
}
