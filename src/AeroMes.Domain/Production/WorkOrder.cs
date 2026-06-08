using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.ValueObjects;

namespace AeroMes.Domain.Production;

public class WorkOrder : AuditableEntity
{
    public int WOID { get; private set; }
    public string WOCode { get; private set; } = string.Empty;        // e.g. WO-2026-0001-CUT
    public int POID { get; private set; }
    public int RoutingStepID { get; private set; }
    public int WorkCenterID { get; private set; }
    public Quantity TargetQuantity { get; private set; } = Quantity.Zero;
    public Quantity ActualQtyOK { get; private set; } = Quantity.Zero;
    public Quantity ActualQtyNG { get; private set; } = Quantity.Zero;
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Prepared;
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    // EF navigations
    public ProductionOrder? ProductionOrder { get; private set; }
    public RoutingStep? RoutingStep { get; private set; }
    public WorkCenter? WorkCenter { get; private set; }

    private WorkOrder() { }

    public static WorkOrder Create(
        string woCode,
        int poId,
        int routingStepId,
        int workCenterId,
        int targetQuantity,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(woCode))
            throw new DomainException("WO code is required.");
        if (targetQuantity <= 0)
            throw new DomainException($"Target quantity must be positive. Got: {targetQuantity}.");

        return new WorkOrder
        {
            WOCode = woCode.Trim().ToUpperInvariant(),
            POID = poId,
            RoutingStepID = routingStepId,
            WorkCenterID = workCenterId,
            TargetQuantity = Quantity.From(targetQuantity),
            ActualQtyOK = Quantity.Zero,
            ActualQtyNG = Quantity.Zero,
            Status = WorkOrderStatus.Prepared,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Start(string operatorId, DateTime? timestamp = null)
    {
        EnsureStatus(WorkOrderStatus.Prepared, nameof(Start));
        Status = WorkOrderStatus.Running;
        ActualStartDate = timestamp ?? DateTime.UtcNow;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderStartedEvent(WOID, WOCode));
    }

    public void Pause(string operatorId)
    {
        EnsureStatus(WorkOrderStatus.Running, nameof(Pause));
        Status = WorkOrderStatus.Paused;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderPausedEvent(WOID, WOCode));
    }

    public void Resume(string operatorId)
    {
        EnsureStatus(WorkOrderStatus.Paused, nameof(Resume));
        Status = WorkOrderStatus.Running;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderResumedEvent(WOID, WOCode));
    }

    public void Complete(string operatorId)
    {
        if (Status is not (WorkOrderStatus.Running or WorkOrderStatus.Paused))
            throw new DomainException(
                $"WorkOrder '{WOCode}' must be Running or Paused to complete. Current: {Status}.");
        Status = WorkOrderStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderCompletedEvent(WOID, WOCode, ActualQtyOK.Value, ActualQtyNG.Value));
    }

    public void AccumulateOutput(int qtyOk, int qtyNg, string operatorId)
    {
        if (Status != WorkOrderStatus.Running)
            throw new DomainException($"WorkOrder '{WOCode}' must be Running to record output. Current: {Status}.");
        ActualQtyOK = ActualQtyOK.Add(qtyOk);
        ActualQtyNG = ActualQtyNG.Add(qtyNg);
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderOutputSubmittedEvent(WOID, WOCode, qtyOk, qtyNg, operatorId));
    }

    private void EnsureStatus(WorkOrderStatus required, string operation)
    {
        if (Status != required)
            throw new DomainException(
                $"WorkOrder '{WOCode}' must be {required} to {operation}. Current: {Status}.");
    }
}
