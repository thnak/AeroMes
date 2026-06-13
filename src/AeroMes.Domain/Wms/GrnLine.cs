using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class GrnLine : Entity
{
    public int GrnLineId { get; private set; }
    public int GrnId { get; private set; }
    public int? PoLineId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal ReceivedQty { get; private set; }
    public DateOnly? ManufacturedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public decimal? GrossWeightKg { get; private set; }
    public QcStatus QcStatus { get; private set; } = QcStatus.Pending;
    public int? DestinationBinId { get; private set; }

    private GrnLine() { }

    internal static GrnLine Create(
        int grnId, int? poLineId, string productCode, string lotNumber,
        decimal receivedQty, DateOnly? manufacturedDate, DateOnly? expiryDate,
        decimal? grossWeightKg, int? destinationBinId)
    {
        return new GrnLine
        {
            GrnId = grnId,
            PoLineId = poLineId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            ReceivedQty = receivedQty,
            ManufacturedDate = manufacturedDate,
            ExpiryDate = expiryDate,
            GrossWeightKg = grossWeightKg,
            QcStatus = QcStatus.Pending,
            DestinationBinId = destinationBinId,
        };
    }

    internal void SetQcStatus(QcStatus status) => QcStatus = status;
}
