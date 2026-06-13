using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class GoodsReceiptNote : AuditableEntity
{
    public int GrnId { get; private set; }
    public string GrnCode { get; private set; } = string.Empty;
    public int? PoId { get; private set; }
    public int StorageLocationId { get; private set; }
    public string ReceivedBy { get; private set; } = string.Empty;
    public DateTime ReceivedAt { get; private set; }
    public GrnStatus Status { get; private set; } = GrnStatus.Draft;
    public string? DeliveryNoteRef { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<GrnLine> _lines = [];
    public IReadOnlyList<GrnLine> Lines => _lines.AsReadOnly();

    private GoodsReceiptNote() { }

    public static GoodsReceiptNote Create(
        string grnCode, int? poId, int storageLocationId,
        string receivedBy, DateTime receivedAt,
        string? deliveryNoteRef, string? notes, string? createdBy)
    {
        return new GoodsReceiptNote
        {
            GrnCode = grnCode.Trim().ToUpperInvariant(),
            PoId = poId,
            StorageLocationId = storageLocationId,
            ReceivedBy = receivedBy.Trim(),
            ReceivedAt = receivedAt,
            Status = GrnStatus.Draft,
            DeliveryNoteRef = deliveryNoteRef?.Trim(),
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public GrnLine AddLine(
        int? poLineId, string productCode, string lotNumber, decimal receivedQty,
        DateOnly? manufacturedDate, DateOnly? expiryDate, decimal? grossWeightKg, int? destinationBinId)
    {
        if (Status != GrnStatus.Draft)
            throw new DomainException($"GRN '{GrnCode}' must be Draft to add lines.");

        var line = GrnLine.Create(GrnId, poLineId, productCode, lotNumber,
            receivedQty, manufacturedDate, expiryDate, grossWeightKg, destinationBinId);
        _lines.Add(line);
        return line;
    }

    public void Confirm(string confirmedBy)
    {
        if (Status != GrnStatus.Draft)
            throw new DomainException($"GRN '{GrnCode}' must be Draft to confirm. Current: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException($"GRN '{GrnCode}' must have at least one line before confirming.");
        Status = GrnStatus.Confirmed;
        Touch(confirmedBy);
    }
}
