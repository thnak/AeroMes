using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Domain.Traceability;

public enum SerialUnitStatus
{
    Active, Shipped, Recalled, Scrapped, Returned, OnHold, Reworked
}

public class SerialUnit : Entity
{
    public Guid SerialID { get; private set; }
    public string SerialNumber { get; private set; } = string.Empty;
    public string? GTIN { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int? WorkOrderID { get; private set; }
    public DateOnly ProductionDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public SerialUnitStatus Status { get; private set; } = SerialUnitStatus.Active;
    public string? UDI { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SerialUnit() { }

    public static SerialUnit Commission(
        string serialNumber,
        string lotNumber,
        string productCode,
        int? workOrderId,
        DateOnly productionDate,
        DateOnly? expiryDate,
        string? gtin = null,
        string? udi = null)
    {
        if (string.IsNullOrWhiteSpace(serialNumber)) throw new DomainException("Serial number is required.");
        if (string.IsNullOrWhiteSpace(lotNumber)) throw new DomainException("Lot number is required.");
        if (string.IsNullOrWhiteSpace(productCode)) throw new DomainException("Product code is required.");

        var unit = new SerialUnit
        {
            SerialID = Guid.NewGuid(),
            SerialNumber = serialNumber.Trim(),
            GTIN = gtin?.Trim(),
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            WorkOrderID = workOrderId,
            ProductionDate = productionDate,
            ExpiryDate = expiryDate,
            Status = SerialUnitStatus.Active,
            UDI = udi?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        unit.RaiseDomainEvent(new SerialCommissionedEvent(
            unit.SerialID, unit.SerialNumber, unit.LotNumber, unit.ProductCode, unit.GTIN, unit.UDI));
        return unit;
    }

    public void Ship(string shipmentRef)
    {
        if (Status == SerialUnitStatus.Recalled)
            throw new DomainException($"Serial {SerialNumber} is recalled and cannot be shipped.");
        if (Status == SerialUnitStatus.Scrapped)
            throw new DomainException($"Serial {SerialNumber} is scrapped and cannot be shipped.");
        Status = SerialUnitStatus.Shipped;
        RaiseDomainEvent(new SerialShippedEvent(SerialID, SerialNumber, LotNumber, shipmentRef));
    }

    public void Recall(Guid recallId)
    {
        if (Status == SerialUnitStatus.Scrapped)
            throw new DomainException($"Serial {SerialNumber} is already scrapped.");
        Status = SerialUnitStatus.Recalled;
        RaiseDomainEvent(new SerialRecalledEvent(SerialID, SerialNumber, LotNumber, recallId));
    }

    public void PlaceOnHold()
    {
        if (Status is SerialUnitStatus.Shipped or SerialUnitStatus.Scrapped)
            throw new DomainException($"Cannot place serial {SerialNumber} on hold in status {Status}.");
        Status = SerialUnitStatus.OnHold;
    }

    public void MarkReturned(string returnRef)
    {
        Status = SerialUnitStatus.Returned;
        RaiseDomainEvent(new SerialReturnedEvent(SerialID, SerialNumber, LotNumber, returnRef));
    }

    public void MarkReworked(string reworkRef)
    {
        Status = SerialUnitStatus.Reworked;
    }

    public void Scrap()
    {
        if (Status == SerialUnitStatus.Shipped)
            throw new DomainException($"Cannot scrap shipped serial {SerialNumber}.");
        Status = SerialUnitStatus.Scrapped;
    }
}
