using AeroMes.Domain.Common;
using AeroMes.Domain.Equipment;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.ValueObjects;

namespace AeroMes.Domain.Production;

public class WorkOrder : AuditableEntity
{
    public int WorkOrderID { get; private set; }
    public string WorkOrderNo { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public WorkOrderStatus Status { get; private set; }
    public Quantity PlannedQty { get; private set; } = Quantity.Zero;
    public Quantity ActualQtyOK { get; private set; } = Quantity.Zero;
    public Quantity ActualQtyNG { get; private set; } = Quantity.Zero;
    public int WorkCenterID { get; private set; }
    public DateTime? PlannedStart { get; private set; }
    public DateTime? PlannedEnd { get; private set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? ActualEnd { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    // EF navigation — for queries only, not domain logic
    public WorkCenter? WorkCenter { get; private set; }

    private WorkOrder() { } // EF constructor

    public static WorkOrder Create(
        string workOrderNo,
        string productCode,
        string productName,
        int plannedQty,
        int workCenterId,
        DateTime? plannedStart = null,
        DateTime? plannedEnd = null,
        string? createdBy = null)
    {
        return new WorkOrder
        {
            WorkOrderNo = workOrderNo,
            ProductCode = productCode,
            ProductName = productName,
            PlannedQty = Quantity.From(plannedQty),
            ActualQtyOK = Quantity.Zero,
            ActualQtyNG = Quantity.Zero,
            WorkCenterID = workCenterId,
            PlannedStart = plannedStart,
            PlannedEnd = plannedEnd,
            Status = WorkOrderStatus.Released,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Start(string operatorId, DateTime? timestamp = null)
    {
        EnsureStatus(WorkOrderStatus.Released, nameof(Start));
        Status = WorkOrderStatus.Running;
        ActualStart = timestamp ?? DateTime.UtcNow;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderStartedEvent(WorkOrderID, WorkOrderNo));
    }

    public void Pause(string operatorId)
    {
        EnsureStatus(WorkOrderStatus.Running, nameof(Pause));
        Status = WorkOrderStatus.Paused;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderPausedEvent(WorkOrderID, WorkOrderNo));
    }

    public void Resume(string operatorId)
    {
        EnsureStatus(WorkOrderStatus.Paused, nameof(Resume));
        Status = WorkOrderStatus.Running;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderResumedEvent(WorkOrderID, WorkOrderNo));
    }

    public void Complete(string operatorId)
    {
        if (Status is not (WorkOrderStatus.Running or WorkOrderStatus.Paused))
            throw new DomainException(
                $"WorkOrder '{WorkOrderNo}' must be Running or Paused to complete. Current: {Status}.");
        Status = WorkOrderStatus.Completed;
        ActualEnd = DateTime.UtcNow;
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderCompletedEvent(
            WorkOrderID, WorkOrderNo, ActualQtyOK.Value, ActualQtyNG.Value));
    }

    public void RecordOutput(int qtyOk, int qtyNg, string operatorId)
    {
        EnsureStatus(WorkOrderStatus.Running, nameof(RecordOutput));
        ActualQtyOK = ActualQtyOK.Add(qtyOk);
        ActualQtyNG = ActualQtyNG.Add(qtyNg);
        Touch(operatorId);
        RaiseDomainEvent(new WorkOrderOutputSubmittedEvent(
            WorkOrderID, WorkOrderNo, qtyOk, qtyNg, operatorId));
    }

    private void EnsureStatus(WorkOrderStatus required, string operation)
    {
        if (Status != required)
            throw new DomainException(
                $"WorkOrder '{WorkOrderNo}' must be {required} to {operation}. Current: {Status}.");
    }
}
