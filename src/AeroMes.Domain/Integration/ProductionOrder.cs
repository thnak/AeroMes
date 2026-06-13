using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Integration;

public enum ProductionOrderStatus { Released, Running, Paused, Completed, Cancelled }

public class ProductionOrder : Entity
{
    public int POID { get; private set; }
    public string POCode { get; private set; } = string.Empty;    // e.g. PO-2026-0001
    public int? SOID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int TargetQuantity { get; private set; }
    public DateTime? PlannedStartDate { get; private set; }
    public DateTime? PlannedEndDate { get; private set; }
    public DateTime? ProductionDeadline { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public ProductionOrderStatus Status { get; private set; } = ProductionOrderStatus.Released;
    public byte Priority { get; private set; } = 5;    // 1-10, lower = higher priority
    public string? AssignedTo { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime SyncedAt { get; private set; }

    // EF navigation
    public SalesOrder? SalesOrder { get; private set; }

    private ProductionOrder() { }

    public static ProductionOrder CreateFromErp(
        string poCode,
        string productCode,
        int targetQuantity,
        int? soId = null,
        DateTime? plannedStart = null,
        DateTime? plannedEnd = null)
    {
        if (targetQuantity <= 0)
            throw new DomainException($"Target quantity must be positive. Got: {targetQuantity}.");

        return new ProductionOrder
        {
            POCode = poCode.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            TargetQuantity = targetQuantity,
            SOID = soId,
            PlannedStartDate = plannedStart,
            PlannedEndDate = plannedEnd,
            Status = ProductionOrderStatus.Released,
            SyncedAt = DateTime.UtcNow,
        };
    }

    public static ProductionOrder CreateInternal(
        string poCode,
        string productCode,
        int targetQuantity,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        DateTime? deadline,
        byte priority,
        string? assignedTo,
        string? createdBy)
    {
        if (targetQuantity <= 0)
            throw new DomainException($"Target quantity must be positive. Got: {targetQuantity}.");

        return new ProductionOrder
        {
            POCode = poCode.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            TargetQuantity = targetQuantity,
            PlannedStartDate = plannedStart,
            PlannedEndDate = plannedEnd,
            ProductionDeadline = deadline,
            Priority = priority,
            AssignedTo = assignedTo,
            Status = ProductionOrderStatus.Released,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            SyncedAt = DateTime.UtcNow,
        };
    }

    public bool IsActive => Status is ProductionOrderStatus.Released or ProductionOrderStatus.Running;

    public void SetRunning()
    {
        if (Status != ProductionOrderStatus.Released)
            throw new DomainException($"PO '{POCode}' must be Released to start. Current: {Status}.");
        Status = ProductionOrderStatus.Running;
        ActualStartDate = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status is not (ProductionOrderStatus.Running or ProductionOrderStatus.Paused))
            throw new DomainException($"PO '{POCode}' must be Running or Paused to complete. Current: {Status}.");
        Status = ProductionOrderStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ProductionOrderStatus.Completed)
            throw new DomainException($"Cannot cancel a completed PO '{POCode}'.");
        Status = ProductionOrderStatus.Cancelled;
    }

    public void Resync() => SyncedAt = DateTime.UtcNow;
}
