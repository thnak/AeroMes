using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Traceability;

public enum LotEventType
{
    Received, Released, OnHold, HoldReleased, Rejected,
    Consumed, Produced, Transferred, Shipped, Returned,
    Scrapped, InspectionPass, InspectionFail,
    StepEntry, StepExit, ParameterCapture,
    AggregationAdd, AggregationRemove,
    RecallFlagged, QuarantineApplied, DispositionDecided
}

public enum LotEventSourceSystem { MES, WMS, LIMS, ERP, Manual, OPC }

public class LotEvent : Entity
{
    public long EventId { get; private set; }
    public LotEventType EventType { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int? WorkOrderID { get; private set; }
    public int? RoutingStepID { get; private set; }
    public int? LocationID { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? UoM { get; private set; }
    public string? Payload { get; private set; }       // JSON event-specific data
    public string OperatorCode { get; private set; } = string.Empty;
    public string? EquipmentCode { get; private set; }
    public DateTime EventTimestamp { get; private set; }    // physical event time
    public DateTime RecordedAt { get; private set; }        // system wall clock
    public LotEventSourceSystem SourceSystem { get; private set; } = LotEventSourceSystem.MES;

    private LotEvent() { }

    public static LotEvent Append(
        LotEventType eventType,
        string lotNumber,
        string productCode,
        string operatorCode,
        DateTime eventTimestamp,
        int? workOrderId = null,
        int? routingStepId = null,
        int? locationId = null,
        decimal? quantity = null,
        string? uom = null,
        string? payload = null,
        string? equipmentCode = null,
        LotEventSourceSystem sourceSystem = LotEventSourceSystem.MES)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new DomainException("Lot number is required.");
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("Product code is required.");
        if (string.IsNullOrWhiteSpace(operatorCode))
            throw new DomainException("Operator code is required.");

        return new LotEvent
        {
            EventType = eventType,
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            WorkOrderID = workOrderId,
            RoutingStepID = routingStepId,
            LocationID = locationId,
            Quantity = quantity,
            UoM = uom?.Trim(),
            Payload = payload,
            OperatorCode = operatorCode.Trim(),
            EquipmentCode = equipmentCode?.Trim(),
            EventTimestamp = eventTimestamp,
            RecordedAt = DateTime.UtcNow,
            SourceSystem = sourceSystem,
        };
    }
}
