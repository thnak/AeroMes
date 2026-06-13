using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class PurchaseOrder : AuditableEntity
{
    public int PoId { get; private set; }
    public string PoCode { get; private set; } = string.Empty;
    public string SupplierCode { get; private set; } = string.Empty;
    public DateOnly ExpectedDeliveryDate { get; private set; }
    public PoStatus Status { get; private set; } = PoStatus.Draft;
    public string? Notes { get; private set; }

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyList<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(
        string poCode, string supplierCode, DateOnly expectedDeliveryDate, string? notes, string? createdBy)
    {
        return new PurchaseOrder
        {
            PoCode = poCode.Trim().ToUpperInvariant(),
            SupplierCode = supplierCode.Trim().ToUpperInvariant(),
            ExpectedDeliveryDate = expectedDeliveryDate,
            Status = PoStatus.Draft,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public PurchaseOrderLine AddLine(string productCode, decimal orderedQty, decimal? unitPrice, string? expectedLotNumber)
    {
        if (Status != PoStatus.Draft)
            throw new DomainException($"PO '{PoCode}' must be Draft to add lines.");
        var line = PurchaseOrderLine.Create(PoId, productCode, orderedQty, unitPrice, expectedLotNumber);
        _lines.Add(line);
        return line;
    }

    public void Confirm(string updatedBy)
    {
        if (Status != PoStatus.Draft)
            throw new DomainException($"PO '{PoCode}' must be Draft to confirm. Current: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException($"PO '{PoCode}' must have at least one line before confirming.");
        Status = PoStatus.Confirmed;
        Touch(updatedBy);
    }

    public void Cancel(string updatedBy)
    {
        if (Status is PoStatus.FullyReceived or PoStatus.Closed)
            throw new DomainException($"PO '{PoCode}' cannot be cancelled in status {Status}.");
        Status = PoStatus.Cancelled;
        Touch(updatedBy);
    }

    public void RecordLineReceived(int poLineId, decimal qty, string updatedBy)
    {
        var line = _lines.FirstOrDefault(l => l.PoLineId == poLineId)
            ?? throw new DomainException($"PurchaseOrderLine '{poLineId}' was not found.");
        line.AddReceived(qty);

        var allFull = _lines.All(l => l.ReceivedQty >= l.OrderedQty);
        var anyReceived = _lines.Any(l => l.ReceivedQty > 0);
        Status = allFull ? PoStatus.FullyReceived : anyReceived ? PoStatus.PartiallyReceived : Status;
        Touch(updatedBy);
    }
}
