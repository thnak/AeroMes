using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability;

public class RecallScopeLot : Entity
{
    public long RecallScopeLotID { get; private set; }
    public Guid RecallID { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public int TraceDepth { get; private set; }
    public LotCategory LotCategory { get; private set; }
    public string? CurrentLocationCode { get; private set; }
    public decimal? QtyOnHand { get; private set; }
    public string? ShipmentRef { get; private set; }
    public string? CustomerRef { get; private set; }
    public Guid? HoldID { get; private set; }
    public DateTime AddedAt { get; private set; }

    private RecallScopeLot() { }

    public static RecallScopeLot Create(
        Guid recallId,
        string lotNumber,
        LotCategory category,
        int traceDepth,
        string? productCode = null,
        string? currentLocationCode = null,
        decimal? qtyOnHand = null,
        string? shipmentRef = null,
        string? customerRef = null)
    {
        return new RecallScopeLot
        {
            RecallID = recallId,
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductCode = productCode?.Trim().ToUpperInvariant(),
            TraceDepth = traceDepth,
            LotCategory = category,
            CurrentLocationCode = currentLocationCode?.Trim(),
            QtyOnHand = qtyOnHand,
            ShipmentRef = shipmentRef?.Trim(),
            CustomerRef = customerRef?.Trim(),
            AddedAt = DateTime.UtcNow,
        };
    }

    public void LinkHold(Guid holdId) => HoldID = holdId;
}
