using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum ReworkStatus { Open, InProgress, Completed, Cancelled }

public class ReworkOrder : AuditableEntity
{
    public int ReworkID { get; private set; }
    public string ReworkCode { get; private set; } = string.Empty;
    public int SourceWOID { get; private set; }
    public long? ScrapTxID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int ReworkQty { get; private set; }
    public int? ReworkStepFromId { get; private set; }
    public ReworkStatus Status { get; private set; } = ReworkStatus.Open;

    public decimal ActMaterialCost { get; private set; }
    public decimal ActLaborCost { get; private set; }
    public decimal ActMachineCost { get; private set; }
    public decimal ActTotalReworkCost { get; private set; }

    private ReworkOrder() { }

    public static ReworkOrder Create(
        string reworkCode, int sourceWoid, long? scrapTxId,
        string productCode, int reworkQty, int? reworkStepFromId,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(reworkCode))
            throw new DomainException("Mã lệnh tái chế không được để trống.");
        if (reworkQty <= 0)
            throw new DomainException("Số lượng tái chế phải lớn hơn 0.");

        return new ReworkOrder
        {
            ReworkCode = reworkCode.Trim().ToUpperInvariant(),
            SourceWOID = sourceWoid,
            ScrapTxID = scrapTxId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ReworkQty = reworkQty,
            ReworkStepFromId = reworkStepFromId,
            Status = ReworkStatus.Open,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Start(string? updatedBy)
    {
        if (Status != ReworkStatus.Open)
            throw new DomainException($"Lệnh tái chế phải ở trạng thái Mở. Hiện tại: {Status}.");
        Status = ReworkStatus.InProgress;
        Touch(updatedBy);
    }

    public void Complete(decimal actMaterialCost, decimal actLaborCost, decimal actMachineCost, string? updatedBy)
    {
        if (Status != ReworkStatus.InProgress)
            throw new DomainException($"Lệnh tái chế phải ở trạng thái Đang thực hiện. Hiện tại: {Status}.");
        ActMaterialCost = actMaterialCost;
        ActLaborCost = actLaborCost;
        ActMachineCost = actMachineCost;
        ActTotalReworkCost = actMaterialCost + actLaborCost + actMachineCost;
        Status = ReworkStatus.Completed;
        Touch(updatedBy);
    }

    public void Cancel(string? updatedBy)
    {
        if (Status == ReworkStatus.Completed || Status == ReworkStatus.Cancelled)
            throw new DomainException($"Không thể hủy lệnh tái chế ở trạng thái {Status}.");
        Status = ReworkStatus.Cancelled;
        Touch(updatedBy);
    }
}
