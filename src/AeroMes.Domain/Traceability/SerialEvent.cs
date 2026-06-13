using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Traceability;

public enum SerialEventType
{
    Commissioned, AssemblyStart, AssemblyComplete,
    InspectionPass, InspectionFail, Packed, Unpacked,
    Shipped, Received, Returned, Recalled, Scrapped,
    Reworked, StatusChanged
}

public class SerialEvent : Entity
{
    public long EventID { get; private set; }
    public SerialEventType EventType { get; private set; }
    public Guid SerialID { get; private set; }
    public int? WorkOrderID { get; private set; }
    public string? LocationCode { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Payload { get; private set; }
    public string? OperatorCode { get; private set; }
    public DateTime EventTimestamp { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private SerialEvent() { }

    public static SerialEvent Log(
        SerialEventType eventType,
        Guid serialId,
        int? workOrderId = null,
        string? locationCode = null,
        decimal? quantity = null,
        string? payload = null,
        string? operatorCode = null,
        DateTime? eventTimestamp = null)
    {
        if (serialId == Guid.Empty) throw new DomainException("Serial ID is required.");

        return new SerialEvent
        {
            EventType = eventType,
            SerialID = serialId,
            WorkOrderID = workOrderId,
            LocationCode = locationCode?.Trim(),
            Quantity = quantity,
            Payload = payload,
            OperatorCode = operatorCode?.Trim(),
            EventTimestamp = eventTimestamp ?? DateTime.UtcNow,
            RecordedAt = DateTime.UtcNow,
        };
    }
}
