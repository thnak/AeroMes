using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class ProductionPlanOrderLine : Entity
{
    public int PlanLineId { get; private set; }
    public int PlanId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public decimal PlannedQty { get; private set; }
    public string? TeamCode { get; private set; }
    public DateTime? PlannedStartDate { get; private set; }
    public DateTime? PlannedEndDate { get; private set; }
    public decimal ActualQty { get; private set; }

    private ProductionPlanOrderLine() { }

    internal static ProductionPlanOrderLine Create(
        int planId, string productCode, decimal plannedQty,
        string? teamCode, DateTime? plannedStart, DateTime? plannedEnd)
    {
        return new ProductionPlanOrderLine
        {
            PlanId = planId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            PlannedQty = plannedQty,
            TeamCode = teamCode?.Trim().ToUpperInvariant(),
            PlannedStartDate = plannedStart,
            PlannedEndDate = plannedEnd,
            ActualQty = 0m,
        };
    }

    internal void UpdateAssignment(string? teamCode, DateTime? plannedStart, DateTime? plannedEnd)
    {
        TeamCode = teamCode?.Trim().ToUpperInvariant();
        PlannedStartDate = plannedStart;
        PlannedEndDate = plannedEnd;
    }

    internal void RecordActual(decimal qty)
    {
        if (qty < 0)
            throw new DomainException("Số lượng thực tế không thể âm.");
        ActualQty = qty;
    }

    public bool IsLate => PlannedEndDate.HasValue && DateTime.UtcNow > PlannedEndDate.Value && ActualQty < PlannedQty;
}
