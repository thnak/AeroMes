using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum BundleStatus { Cut, Sewing, Finished, Rejected }

public class Bundle : Entity
{
    public int BundleID { get; private set; }
    public int CutOrderID { get; private set; }
    public string SizeCode { get; private set; } = string.Empty;
    public int BundleNumber { get; private set; }
    public int PieceCount { get; private set; }
    public BundleStatus Status { get; private set; } = BundleStatus.Cut;
    public DateTime CreatedAt { get; private set; }

    private Bundle() { }

    public static Bundle Create(int cutOrderId, string sizeCode, int bundleNumber, int pieceCount)
    {
        if (pieceCount <= 0) throw new DomainException("Bundle piece count must be positive.");
        return new Bundle
        {
            CutOrderID = cutOrderId,
            SizeCode = sizeCode.Trim().ToUpperInvariant(),
            BundleNumber = bundleNumber,
            PieceCount = pieceCount,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateStatus(BundleStatus status) => Status = status;
}
