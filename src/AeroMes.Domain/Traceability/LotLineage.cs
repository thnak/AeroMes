using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Traceability;

public enum LineageType { Consume, Transform, Rework, Split, Merge }

public class LotLineage : Entity
{
    public long LineageId { get; private set; }
    public string ParentLotNumber { get; private set; } = string.Empty;
    public string ChildLotNumber { get; private set; } = string.Empty;
    public int? WorkOrderID { get; private set; }
    public int? RoutingStepID { get; private set; }
    public LineageType LineageType { get; private set; } = LineageType.Consume;
    public decimal? QuantityConsumed { get; private set; }
    public string? UoM { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private LotLineage() { }

    public static LotLineage Record(
        string parentLotNumber,
        string childLotNumber,
        LineageType lineageType,
        int? workOrderId,
        int? routingStepId,
        decimal? quantityConsumed,
        string? uom)
    {
        if (string.IsNullOrWhiteSpace(parentLotNumber))
            throw new DomainException("Parent lot number is required.");
        if (string.IsNullOrWhiteSpace(childLotNumber))
            throw new DomainException("Child lot number is required.");
        if (parentLotNumber == childLotNumber)
            throw new DomainException("Parent and child lot numbers must be different.");
        if (quantityConsumed.HasValue && quantityConsumed.Value <= 0)
            throw new DomainException("Quantity consumed must be positive.");

        return new LotLineage
        {
            ParentLotNumber = parentLotNumber.Trim().ToUpperInvariant(),
            ChildLotNumber = childLotNumber.Trim().ToUpperInvariant(),
            LineageType = lineageType,
            WorkOrderID = workOrderId,
            RoutingStepID = routingStepId,
            QuantityConsumed = quantityConsumed,
            UoM = uom?.Trim(),
            RecordedAt = DateTime.UtcNow,
        };
    }
}
