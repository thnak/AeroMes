using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Events;

namespace AeroMes.Domain.Wms;

public class StockMovement : Entity
{
    public long MovementId { get; private set; }
    public MovementType MovementType { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public int LocationId { get; private set; }
    public int? BinId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    public static StockMovement CreateReceive(
        string productCode, string lotNumber, decimal quantity,
        int locationId, int? binId, string reference, string? notes, string? createdBy)
    {
        var m = new StockMovement
        {
            MovementType = MovementType.Receive,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            Quantity = quantity,
            LocationId = locationId,
            BinId = binId,
            Reference = reference,
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        m.RaiseDomainEvent(new StockMovementCreatedEvent(m.ProductCode, locationId, MovementType.Receive, quantity));
        return m;
    }

    public static StockMovement CreateAdjust(
        string productCode, string lotNumber, decimal delta,
        int locationId, int? binId, string reference, string? notes, string? createdBy)
    {
        var m = new StockMovement
        {
            MovementType = MovementType.Adjust,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            Quantity = delta,
            LocationId = locationId,
            BinId = binId,
            Reference = reference,
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        m.RaiseDomainEvent(new StockMovementCreatedEvent(m.ProductCode, locationId, MovementType.Adjust, delta));
        return m;
    }

    public static StockMovement CreateIssue(
        string productCode, string lotNumber, decimal quantity,
        int locationId, int? binId, string reference, string? notes, string? createdBy)
    {
        if (quantity <= 0)
            throw new DomainException("Issue quantity must be positive.");
        var m = new StockMovement
        {
            MovementType = MovementType.Issue,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            Quantity = quantity,
            LocationId = locationId,
            BinId = binId,
            Reference = reference,
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        m.RaiseDomainEvent(new StockMovementCreatedEvent(m.ProductCode, locationId, MovementType.Issue, quantity));
        return m;
    }
}
