using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum BundleStatus { Cut, AtSewing, SewingComplete, AtFinishing, AtQC, QCPass, QCFail, Packed }

public class Bundle : Entity
{
    public int BundleID { get; private set; }
    public string BundleBarcode { get; private set; } = string.Empty;
    public int CutOrderID { get; private set; }
    public string StyleCode { get; private set; } = string.Empty;
    public string ColorCode { get; private set; } = string.Empty;
    public string SizeCode { get; private set; } = string.Empty;
    public int BundleNumber { get; private set; }
    public int Quantity { get; private set; }
    public string? CurrentOperationCode { get; private set; }
    public int? CurrentWorkCenterID { get; private set; }
    public BundleStatus Status { get; private set; } = BundleStatus.Cut;
    public int QtyOKCumulative { get; private set; }
    public int QtyNGCumulative { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Bundle() { }

    public static Bundle Create(
        int cutOrderId, string styleCode, string colorCode, string sizeCode,
        int bundleNumber, int quantity, string barcode)
    {
        if (quantity <= 0) throw new DomainException("Bundle quantity must be positive.");
        return new Bundle
        {
            CutOrderID = cutOrderId,
            StyleCode = styleCode.Trim().ToUpperInvariant(),
            ColorCode = colorCode.Trim().ToUpperInvariant(),
            SizeCode = sizeCode.Trim().ToUpperInvariant(),
            BundleNumber = bundleNumber,
            Quantity = quantity,
            BundleBarcode = barcode.Trim().ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void ReceiveAtStation(string operationCode, int workCenterId)
    {
        CurrentOperationCode = operationCode.Trim().ToUpperInvariant();
        CurrentWorkCenterID = workCenterId;
        Status = BundleStatus.AtSewing;
    }

    public void AdvanceStatus(string nextOperationCode, int nextWorkCenterId, int qtyOK, int qtyNG)
    {
        QtyOKCumulative += qtyOK;
        QtyNGCumulative += qtyNG;
        CurrentOperationCode = nextOperationCode.Trim().ToUpperInvariant();
        CurrentWorkCenterID = nextWorkCenterId;
    }

    public void SetStatus(BundleStatus status) => Status = status;

    public void Rework(string targetOperationCode)
    {
        if (Status == BundleStatus.Packed)
            throw new DomainException("Cannot rework a packed bundle.");
        CurrentOperationCode = targetOperationCode.Trim().ToUpperInvariant();
        Status = BundleStatus.AtSewing;
    }

    public void UpdateStatus(BundleStatus status) => Status = status;
}
