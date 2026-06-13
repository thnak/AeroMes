using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;

namespace AeroMes.Domain.Integration;

public enum SalesOrderStatus
{
    Unconfirmed,
    Open,
    IncompleteCmds,
    InProduction,
    Paused,
    Overdue,
    Closed,
    Cancelled
}

public enum SoSyncSource { Manual, Erp }

public class SalesOrder : AuditableEntity
{
    public int SOID { get; private set; }
    public string SOCode { get; private set; } = string.Empty;
    public string? CustomerName { get; private set; }
    public string? CustomerCode { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Open;
    public SoSyncSource SyncSource { get; private set; } = SoSyncSource.Erp;
    public string? FacilityCode { get; private set; }
    public string? ConfirmedBy { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime SyncedAt { get; private set; }

    public Customer? Customer { get; private set; }

    private readonly List<SalesOrderLine> _lines = [];
    public IReadOnlyList<SalesOrderLine> Lines => _lines.AsReadOnly();

    private SalesOrder() { }

    public static SalesOrder CreateFromErp(
        string soCode,
        DateTime orderDate,
        string? customerName = null,
        DateTime? deliveryDate = null,
        string? customerCode = null)
    {
        return new SalesOrder
        {
            SOCode = soCode.Trim().ToUpperInvariant(),
            CustomerName = customerName,
            CustomerCode = customerCode?.Trim().ToUpperInvariant(),
            OrderDate = orderDate,
            DeliveryDate = deliveryDate,
            Status = SalesOrderStatus.Open,
            SyncSource = SoSyncSource.Erp,
            SyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static SalesOrder CreateManual(
        string soCode, string? customerCode, string? customerName,
        DateTime orderDate, DateTime? deliveryDate, string? notes, string? createdBy)
    {
        return new SalesOrder
        {
            SOCode = soCode.Trim().ToUpperInvariant(),
            CustomerCode = customerCode?.Trim().ToUpperInvariant(),
            CustomerName = customerName,
            OrderDate = orderDate,
            DeliveryDate = deliveryDate,
            Notes = notes,
            Status = SalesOrderStatus.Unconfirmed,
            SyncSource = SoSyncSource.Manual,
            SyncedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddLine(SalesOrderLine line) => _lines.Add(line);

    public void ReplaceLines(IReadOnlyList<SalesOrderLine> lines, string? updatedBy)
    {
        if (Status is SalesOrderStatus.Cancelled or SalesOrderStatus.Closed)
            throw new DomainException($"Cannot modify lines on a {Status} order.");
        _lines.Clear();
        _lines.AddRange(lines);
        Touch(updatedBy);
    }

    public void Confirm(string? facilityCode, string confirmedBy)
    {
        if (Status != SalesOrderStatus.Unconfirmed)
            throw new DomainException($"Only Unconfirmed orders can be confirmed. Current: {Status}.");
        FacilityCode = facilityCode;
        ConfirmedBy = confirmedBy;
        ConfirmedAt = DateTime.UtcNow;
        Status = SalesOrderStatus.Open;
        Touch(confirmedBy);
    }

    public void Reject(string rejectedBy)
    {
        if (Status is SalesOrderStatus.Cancelled or SalesOrderStatus.Closed)
            throw new DomainException($"Order is already {Status}.");
        Status = SalesOrderStatus.Cancelled;
        Touch(rejectedBy);
    }

    public void SetInProduction(string? updatedBy)
    {
        Status = SalesOrderStatus.InProduction;
        Touch(updatedBy);
    }

    public void SetPaused(string? updatedBy)
    {
        Status = SalesOrderStatus.Paused;
        Touch(updatedBy);
    }

    public void SetOverdue(string? updatedBy)
    {
        Status = SalesOrderStatus.Overdue;
        Touch(updatedBy);
    }

    public void Complete(string? updatedBy)
    {
        if (Status is SalesOrderStatus.Cancelled or SalesOrderStatus.Closed)
            throw new DomainException($"Order is already {Status}.");
        Status = SalesOrderStatus.Closed;
        Touch(updatedBy);
    }

    public void LinkCustomer(string? customerCode) =>
        CustomerCode = customerCode?.Trim().ToUpperInvariant();

    public void Close() => Status = SalesOrderStatus.Closed;
    public void Cancel() => Status = SalesOrderStatus.Cancelled;
    public void Resync() => SyncedAt = DateTime.UtcNow;
}

public class SalesOrderLine
{
    public int LineId { get; private set; }
    public int SOID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? ProductName { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Unit { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }

    private SalesOrderLine() { }

    public static SalesOrderLine Create(
        int soid, string productCode, string? productName,
        decimal quantity, string? unit, decimal unitPrice = 0, string? notes = null)
    {
        return new SalesOrderLine
        {
            SOID = soid,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName?.Trim(),
            Quantity = quantity,
            Unit = unit,
            UnitPrice = unitPrice,
            Notes = notes
        };
    }

    public void UpdateQuantity(decimal quantity) => Quantity = quantity;
}
