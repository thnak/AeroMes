using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum InspectionVoucherType { PostProduction, PostOutsourcing }
public enum InspectionVoucherStatus { NotStarted, InProgress, Completed }
public enum InspectionConclusion { Pass, Fail, Pending }

public class QualityInspectionVoucher : AuditableEntity
{
    public int VoucherID { get; private set; }
    public string VoucherNumber { get; private set; } = string.Empty;
    public string VoucherName { get; private set; } = string.Empty;
    public InspectionVoucherType InspectionType { get; private set; }
    public string InspectorName { get; private set; } = string.Empty;
    public DateOnly InspectionDate { get; private set; }
    public int? LinkedRequestId { get; private set; }
    public int? ProductionOrderId { get; private set; }
    public decimal SampleQuantity { get; private set; }
    public decimal PassingSamples { get; private set; }
    public decimal FailingSamples { get; private set; }
    public InspectionConclusion Conclusion { get; private set; } = InspectionConclusion.Pending;
    public InspectionVoucherStatus Status { get; private set; } = InspectionVoucherStatus.NotStarted;

    private readonly List<VoucherDefectDetail> _defects = [];
    public IReadOnlyList<VoucherDefectDetail> Defects => _defects.AsReadOnly();

    private QualityInspectionVoucher() { }

    public static QualityInspectionVoucher Create(
        string voucherNumber, string voucherName,
        InspectionVoucherType inspectionType, string inspectorName,
        DateOnly inspectionDate, int? linkedRequestId, int? productionOrderId,
        decimal sampleQuantity, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(voucherNumber))
            throw new DomainException("Số phiếu kiểm tra không được để trống.");
        if (sampleQuantity <= 0)
            throw new DomainException("Số lượng mẫu phải lớn hơn 0.");

        return new QualityInspectionVoucher
        {
            VoucherNumber = voucherNumber.Trim().ToUpperInvariant(),
            VoucherName = voucherName.Trim(),
            InspectionType = inspectionType,
            InspectorName = inspectorName.Trim(),
            InspectionDate = inspectionDate,
            LinkedRequestId = linkedRequestId,
            ProductionOrderId = productionOrderId,
            SampleQuantity = sampleQuantity,
            PassingSamples = 0,
            FailingSamples = 0,
            Conclusion = InspectionConclusion.Pending,
            Status = InspectionVoucherStatus.NotStarted,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void AddDefect(int defectCodeId, string defectName, decimal quantity, string? updatedBy)
    {
        _defects.Add(VoucherDefectDetail.Create(VoucherID, defectCodeId, defectName, quantity));
        FailingSamples = _defects.Sum(d => d.Quantity);
        PassingSamples = SampleQuantity - FailingSamples;
        if (PassingSamples < 0) PassingSamples = 0;
        Touch(updatedBy);
    }

    public void ClearDefects(string? updatedBy)
    {
        _defects.Clear();
        FailingSamples = 0;
        PassingSamples = SampleQuantity;
        Touch(updatedBy);
    }

    public void SetConclusion(InspectionConclusion conclusion, string? updatedBy)
    {
        Conclusion = conclusion;
        Touch(updatedBy);
    }

    public void Start(string? updatedBy)
    {
        if (Status != InspectionVoucherStatus.NotStarted)
            throw new DomainException($"Phiếu phải ở trạng thái Chưa bắt đầu. Hiện tại: {Status}.");
        Status = InspectionVoucherStatus.InProgress;
        Touch(updatedBy);
    }

    public void Complete(string? updatedBy)
    {
        if (Status != InspectionVoucherStatus.InProgress)
            throw new DomainException($"Phiếu phải ở trạng thái Đang thực hiện. Hiện tại: {Status}.");
        if (Conclusion == InspectionConclusion.Pending)
            Conclusion = FailingSamples > 0 ? InspectionConclusion.Fail : InspectionConclusion.Pass;
        Status = InspectionVoucherStatus.Completed;
        Touch(updatedBy);
    }
}
