using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum MpsGranularity { Quarter, Month, Week }
public enum MpsDataSource { DemandForecast, SalesOrder }
public enum MpsStatus { Draft, Approved, Closed }
public enum MpsDistributionStrategy { Forward, Backward }

public class MasterProductionPlan : AuditableEntity
{
    public int MasterPlanId { get; private set; }
    public string PlanNumber { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public string? OrganizationalUnit { get; private set; }
    public MpsGranularity Granularity { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public MpsDataSource DataSource { get; private set; }
    public decimal WorkingHoursPerDay { get; private set; }
    public int WorkingDaysPerWeek { get; private set; }
    public MpsStatus Status { get; private set; }

    private readonly List<MasterPlanLine> _lines = [];
    public IReadOnlyList<MasterPlanLine> Lines => _lines.AsReadOnly();

    private MasterProductionPlan() { }

    public static MasterProductionPlan Create(
        string planNumber, string planName, string? organizationalUnit,
        MpsGranularity granularity, DateOnly periodStart, DateOnly periodEnd,
        MpsDataSource dataSource, decimal workingHoursPerDay, int workingDaysPerWeek,
        string? createdBy)
    {
        if (periodEnd <= periodStart)
            throw new DomainException("PeriodEnd must be after PeriodStart.");
        if (workingHoursPerDay <= 0 || workingHoursPerDay > 24)
            throw new DomainException("WorkingHoursPerDay must be between 1 and 24.");
        if (workingDaysPerWeek < 1 || workingDaysPerWeek > 7)
            throw new DomainException("WorkingDaysPerWeek must be between 1 and 7.");

        return new MasterProductionPlan
        {
            PlanNumber = planNumber.Trim().ToUpperInvariant(),
            PlanName = planName.Trim(),
            OrganizationalUnit = organizationalUnit?.Trim(),
            Granularity = granularity,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            DataSource = dataSource,
            WorkingHoursPerDay = workingHoursPerDay,
            WorkingDaysPerWeek = workingDaysPerWeek,
            Status = MpsStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string planName, string? organizationalUnit,
        decimal workingHoursPerDay, int workingDaysPerWeek, string? updatedBy)
    {
        if (Status == MpsStatus.Closed)
            throw new DomainException("Cannot modify a closed plan.");
        PlanName = planName.Trim();
        OrganizationalUnit = organizationalUnit?.Trim();
        WorkingHoursPerDay = workingHoursPerDay;
        WorkingDaysPerWeek = workingDaysPerWeek;
        Touch(updatedBy);
    }

    public void ReplaceLines(IReadOnlyList<MasterPlanLine> lines, string? updatedBy)
    {
        if (Status == MpsStatus.Closed)
            throw new DomainException("Cannot modify a closed plan.");
        _lines.Clear();
        _lines.AddRange(lines);
        Touch(updatedBy);
    }

    public void AddLine(MasterPlanLine line)
    {
        if (Status == MpsStatus.Closed)
            throw new DomainException("Cannot modify a closed plan.");
        _lines.Add(line);
    }

    public void Approve(string updatedBy)
    {
        if (Status != MpsStatus.Draft)
            throw new DomainException($"Only Draft plans can be approved. Current status: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException("Plan must have at least one product line before approval.");
        Status = MpsStatus.Approved;
        Touch(updatedBy);
    }

    public void Close(string updatedBy)
    {
        if (Status == MpsStatus.Closed)
            throw new DomainException("Plan is already closed.");
        Status = MpsStatus.Closed;
        Touch(updatedBy);
    }
}

public class MasterPlanLine
{
    public int LineId { get; private set; }
    public int MasterPlanId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? ProductName { get; private set; }
    public string? UnitOfMeasure { get; private set; }
    public decimal QuantityRequired { get; private set; }
    public decimal PlannedQuantity { get; private set; }
    public decimal DailyCapacity { get; private set; }
    public decimal OpeningInventory { get; private set; }
    public decimal ClosingInventoryForecast { get; private set; }
    public MpsDistributionStrategy DistributionStrategy { get; private set; }

    private MasterPlanLine() { }

    public static MasterPlanLine Create(
        int masterPlanId, string productCode, string? productName,
        string? unitOfMeasure, decimal quantityRequired, decimal dailyCapacity,
        decimal openingInventory, MpsDistributionStrategy strategy)
    {
        var line = new MasterPlanLine
        {
            MasterPlanId = masterPlanId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName?.Trim(),
            UnitOfMeasure = unitOfMeasure,
            QuantityRequired = quantityRequired,
            PlannedQuantity = 0,
            DailyCapacity = dailyCapacity,
            OpeningInventory = openingInventory,
            DistributionStrategy = strategy
        };
        line.RecalculateClosingInventory();
        return line;
    }

    public void UpdatePlannedQuantity(decimal plannedQty)
    {
        PlannedQuantity = plannedQty;
        RecalculateClosingInventory();
    }

    public void RecalculateClosingInventory()
    {
        ClosingInventoryForecast = OpeningInventory + PlannedQuantity - QuantityRequired;
    }
}
