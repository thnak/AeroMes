using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum DppGranularity { Day, Shift }
public enum DppStatus { Draft, Finalized }
public enum DppDistributionStrategy { EvenSpread, Forward, Backward }

public class DetailedProductionPlan : AuditableEntity
{
    public int DetailPlanId { get; private set; }
    public string PlanNumber { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public int MasterPlanId { get; private set; }
    public string? OrganizationalUnit { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public DppGranularity Granularity { get; private set; }
    public DppStatus Status { get; private set; }
    public bool HasProductionOrders { get; private set; }

    private readonly List<DppProductLine> _productLines = [];
    public IReadOnlyList<DppProductLine> ProductLines => _productLines.AsReadOnly();

    private DetailedProductionPlan() { }

    public static DetailedProductionPlan Create(
        string planNumber, string planName, int masterPlanId, string? organizationalUnit,
        DateOnly periodStart, DateOnly periodEnd, DppGranularity granularity, string? createdBy)
    {
        if (periodEnd <= periodStart)
            throw new DomainException("PeriodEnd must be after PeriodStart.");
        return new DetailedProductionPlan
        {
            PlanNumber = planNumber.Trim().ToUpperInvariant(),
            PlanName = planName.Trim(),
            MasterPlanId = masterPlanId,
            OrganizationalUnit = organizationalUnit?.Trim(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Granularity = granularity,
            Status = DppStatus.Draft,
            HasProductionOrders = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string planName, DppGranularity granularity, string? updatedBy)
    {
        if (HasProductionOrders)
            throw new DomainException("Cannot modify a plan that has production orders.");
        if (Status == DppStatus.Finalized)
            throw new DomainException("Cannot modify a finalized plan.");
        PlanName = planName.Trim();
        Granularity = granularity;
        Touch(updatedBy);
    }

    public void AddProductLine(DppProductLine line)
    {
        if (HasProductionOrders)
            throw new DomainException("Cannot modify a plan that has production orders.");
        _productLines.Add(line);
    }

    public void ReplaceProductLines(IReadOnlyList<DppProductLine> lines, string? updatedBy)
    {
        if (HasProductionOrders)
            throw new DomainException("Cannot modify a plan that has production orders.");
        _productLines.Clear();
        _productLines.AddRange(lines);
        Touch(updatedBy);
    }

    public void Finalize(string updatedBy)
    {
        if (Status == DppStatus.Finalized)
            throw new DomainException("Plan is already finalized.");
        if (_productLines.Count == 0)
            throw new DomainException("Plan must have at least one product line before finalizing.");
        Status = DppStatus.Finalized;
        Touch(updatedBy);
    }

    public void MarkProductionOrdersGenerated()
    {
        HasProductionOrders = true;
    }
}

public class DppProductLine
{
    public int DppLineId { get; private set; }
    public int DetailPlanId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? ProductName { get; private set; }
    public string? UnitOfMeasure { get; private set; }
    public decimal TotalRequiredQty { get; private set; }
    public decimal DailyCapacity { get; private set; }

    private readonly List<DppSlot> _slots = [];
    public IReadOnlyList<DppSlot> Slots => _slots.AsReadOnly();

    private DppProductLine() { }

    public static DppProductLine Create(
        int detailPlanId, string productCode, string? productName,
        string? unitOfMeasure, decimal totalRequiredQty, decimal dailyCapacity)
    {
        return new DppProductLine
        {
            DetailPlanId = detailPlanId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName?.Trim(),
            UnitOfMeasure = unitOfMeasure,
            TotalRequiredQty = totalRequiredQty,
            DailyCapacity = dailyCapacity
        };
    }

    public void UpdateCapacity(decimal dailyCapacity) => DailyCapacity = dailyCapacity;

    public void ClearSlots() => _slots.Clear();

    public void AddSlot(DppSlot slot) => _slots.Add(slot);
}

public class DppSlot
{
    public int SlotId { get; private set; }
    public int DppLineId { get; private set; }
    public DateOnly SlotDate { get; private set; }
    public string? ShiftLabel { get; private set; }
    public decimal AllocatedQty { get; private set; }

    private DppSlot() { }

    public static DppSlot Create(int dppLineId, DateOnly slotDate, string? shiftLabel, decimal allocatedQty)
        => new()
        {
            DppLineId = dppLineId,
            SlotDate = slotDate,
            ShiftLabel = shiftLabel,
            AllocatedQty = allocatedQty
        };

    public void AdjustQty(decimal qty) => AllocatedQty = qty;
}
