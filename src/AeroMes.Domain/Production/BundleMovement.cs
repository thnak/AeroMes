using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class BundleMovement : Entity
{
    public long MovementID { get; private set; }
    public int BundleID { get; private set; }
    public string OperationCode { get; private set; } = string.Empty;
    public int WorkCenterID { get; private set; }
    public string OperatorID { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public int QtyOK { get; private set; }
    public int QtyNG { get; private set; }
    public string? DefectCodes { get; private set; }
    public decimal? SAM_Minutes { get; private set; }
    public decimal? ActualMinsPerPiece { get; private set; }  // SQL computed column
    public decimal? EfficiencyPct { get; private set; }       // SQL computed column

    private BundleMovement() { }

    public static BundleMovement Open(
        int bundleId, string operationCode, int workCenterId, string operatorId, decimal? samMinutes)
        => new()
        {
            BundleID = bundleId,
            OperationCode = operationCode.Trim().ToUpperInvariant(),
            WorkCenterID = workCenterId,
            OperatorID = operatorId.Trim(),
            StartTime = DateTime.UtcNow,
            SAM_Minutes = samMinutes,
        };

    public void Close(int qtyOK, int qtyNG, string? defectCodesJson)
    {
        if (EndTime.HasValue) throw new DomainException("This movement is already closed.");
        if (qtyOK < 0 || qtyNG < 0) throw new DomainException("Quantities cannot be negative.");
        EndTime = DateTime.UtcNow;
        QtyOK = qtyOK;
        QtyNG = qtyNG;
        DefectCodes = defectCodesJson;
    }

    public bool IsOpen => EndTime is null;
}
