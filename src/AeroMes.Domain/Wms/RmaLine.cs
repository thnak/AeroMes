using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class RmaLine : Entity
{
    public int RmaLineId { get; private set; }
    public int RmaId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal ReturnQty { get; private set; }
    public decimal ReceivedQty { get; private set; }
    public RmaDisposition? Disposition { get; private set; }
    public int? NcrId { get; private set; }
    public long? StockMovementId { get; private set; }

    private RmaLine() { }

    internal static RmaLine Create(int rmaId, string productCode, string lotNumber, decimal returnQty)
    {
        if (returnQty <= 0)
            throw new DomainException("Số lượng trả về phải lớn hơn 0.");

        return new RmaLine
        {
            RmaId = rmaId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            ReturnQty = returnQty,
            ReceivedQty = 0,
        };
    }

    internal void RecordReceived(decimal qty)
    {
        if (qty < 0)
            throw new DomainException("Số lượng nhận thực tế không được âm.");
        ReceivedQty = qty;
    }

    internal void Dispose(RmaDisposition disposition, long? movementId)
    {
        if (Disposition.HasValue)
            throw new DomainException($"Dòng #{RmaLineId} đã được xử lý disposition: {Disposition}.");
        Disposition = disposition;
        StockMovementId = movementId;
    }

    internal void LinkNcr(int ncrId)
    {
        NcrId = ncrId;
    }
}
