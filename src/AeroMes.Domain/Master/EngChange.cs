using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum EcType { Ecr, Eco }
public enum EcReason { DesignImprovement, CostReduction, SupplierChange, DefectCorrection, Regulatory, Other }
public enum EcStatus { Open, UnderReview, Approved, Rejected, Implemented, Cancelled }
public enum EcPriority { Critical, High, Normal, Low }

/// <summary>Engineering Change Request (ECR) / Engineering Change Order (ECO).</summary>
public class EngChange : AuditableEntity
{
    public int EcId { get; private set; }
    public string EcNumber { get; private set; } = string.Empty;  // ECR-2026-001
    public EcType EcType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public EcReason Reason { get; private set; }
    public EcStatus Status { get; private set; } = EcStatus.Open;
    public EcPriority Priority { get; private set; } = EcPriority.Normal;
    public string? RequestedBy { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? AffectedProducts { get; private set; }         // comma-separated ProductCodes
    public string? SourceEcrNumber { get; private set; }          // the ECR this ECO was promoted from
    public int? NewBomHeaderId { get; private set; }              // BOM version created by this ECO

    private EngChange() { }

    public static EngChange Create(
        string ecNumber, EcType ecType, string title, string? description,
        EcReason reason, EcPriority priority, DateOnly? targetDate,
        string? affectedProducts, string? sourceEcrNumber, string? requestedBy)
    {
        return new EngChange
        {
            EcNumber = ecNumber.Trim().ToUpperInvariant(),
            EcType = ecType,
            Title = title.Trim(),
            Description = description,
            Reason = reason,
            Priority = priority,
            TargetDate = targetDate,
            AffectedProducts = affectedProducts,
            SourceEcrNumber = sourceEcrNumber?.Trim().ToUpperInvariant(),
            RequestedBy = requestedBy,
            RequestedAt = DateTime.UtcNow,
            Status = EcStatus.Open,
            CreatedBy = requestedBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SubmitForReview(string? updatedBy)
    {
        EnsureStatus(EcStatus.Open, "gửi duyệt");
        Status = EcStatus.UnderReview;
        Touch(updatedBy);
    }

    public void Approve(string? approvedBy)
    {
        if (Status is not (EcStatus.Open or EcStatus.UnderReview))
            throw new DomainException(
                $"Phiếu '{EcNumber}' phải ở trạng thái Open hoặc UnderReview để phê duyệt. Trạng thái hiện tại: {Status}.");
        Status = EcStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        Touch(approvedBy);
    }

    public void Reject(string? updatedBy)
    {
        if (Status is not (EcStatus.Open or EcStatus.UnderReview))
            throw new DomainException(
                $"Phiếu '{EcNumber}' phải ở trạng thái Open hoặc UnderReview để từ chối. Trạng thái hiện tại: {Status}.");
        Status = EcStatus.Rejected;
        Touch(updatedBy);
    }

    public void Cancel(string? updatedBy)
    {
        if (Status == EcStatus.Implemented)
            throw new DomainException($"Phiếu '{EcNumber}' đã được triển khai, không thể hủy.");
        Status = EcStatus.Cancelled;
        Touch(updatedBy);
    }

    /// <summary>Pre-flight check so callers can validate before creating side effects (e.g. the BOM draft).</summary>
    public void EnsureCanImplement()
    {
        if (EcType != EcType.Eco)
            throw new DomainException($"Chỉ ECO mới được triển khai. Phiếu '{EcNumber}' là {EcType}.");
        EnsureStatus(EcStatus.Approved, "triển khai");
    }

    public void MarkImplemented(int newBomHeaderId, string? updatedBy)
    {
        EnsureCanImplement();
        Status = EcStatus.Implemented;
        NewBomHeaderId = newBomHeaderId;
        Touch(updatedBy);
    }

    private void EnsureStatus(EcStatus required, string operation)
    {
        if (Status != required)
            throw new DomainException(
                $"Phiếu '{EcNumber}' phải ở trạng thái {required} để {operation}. Trạng thái hiện tại: {Status}.");
    }
}
