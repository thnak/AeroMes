using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum PurchaseRequestStatus { NotSent, Submitted, Approved, Rejected }
public enum PurchaseRequestSourceType { Manual, MaterialPlan, SalesOrder }

public class MaterialPurchaseRequest : AuditableEntity
{
    public int RequestID { get; private set; }
    public string RequestNumber { get; private set; } = string.Empty;
    public DateOnly CreationDate { get; private set; }
    public string Requestor { get; private set; } = string.Empty;
    public string? RequestingUnit { get; private set; }
    public DateOnly? Deadline { get; private set; }
    public string? ProcurementPurpose { get; private set; }
    public PurchaseRequestStatus Status { get; private set; } = PurchaseRequestStatus.NotSent;
    public PurchaseRequestSourceType SourceType { get; private set; }
    public int? SourceReferenceId { get; private set; }
    public string? SalesOrderCode { get; private set; }

    private readonly List<MaterialPurchaseRequestLine> _lines = [];
    public IReadOnlyList<MaterialPurchaseRequestLine> Lines => _lines.AsReadOnly();

    private MaterialPurchaseRequest() { }

    public static MaterialPurchaseRequest Create(
        string requestNumber, string requestor, string? requestingUnit,
        DateOnly? deadline, string? procurementPurpose,
        PurchaseRequestSourceType sourceType, int? sourceReferenceId,
        string? salesOrderCode, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(requestNumber))
            throw new DomainException("Số yêu cầu không được để trống.");
        if (string.IsNullOrWhiteSpace(requestor))
            throw new DomainException("Người yêu cầu không được để trống.");

        return new MaterialPurchaseRequest
        {
            RequestNumber = requestNumber.Trim().ToUpperInvariant(),
            CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Requestor = requestor.Trim(),
            RequestingUnit = requestingUnit?.Trim(),
            Deadline = deadline,
            ProcurementPurpose = procurementPurpose?.Trim(),
            Status = PurchaseRequestStatus.NotSent,
            SourceType = sourceType,
            SourceReferenceId = sourceReferenceId,
            SalesOrderCode = salesOrderCode?.Trim().ToUpperInvariant(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public MaterialPurchaseRequestLine AddLine(
        string materialCode, string materialName, string unitOfMeasure,
        decimal requiredQty, string? updatedBy)
    {
        if (Status == PurchaseRequestStatus.Approved)
            throw new DomainException("Không thể thêm dòng vào yêu cầu đã được phê duyệt.");
        if (requiredQty <= 0)
            throw new DomainException("Số lượng yêu cầu phải lớn hơn 0.");

        var line = MaterialPurchaseRequestLine.Create(
            RequestID, materialCode, materialName, unitOfMeasure, requiredQty);
        _lines.Add(line);
        Touch(updatedBy);
        return line;
    }

    public void RemoveLine(int lineId, string? updatedBy)
    {
        if (Status == PurchaseRequestStatus.Approved)
            throw new DomainException("Không thể xóa dòng khỏi yêu cầu đã được phê duyệt.");
        var line = _lines.FirstOrDefault(l => l.LineID == lineId)
            ?? throw new DomainException($"Dòng yêu cầu '{lineId}' không tồn tại.");
        _lines.Remove(line);
        Touch(updatedBy);
    }

    public void UpdateHeader(
        string requestor, string? requestingUnit, DateOnly? deadline,
        string? procurementPurpose, string? updatedBy)
    {
        if (Status == PurchaseRequestStatus.Approved)
            throw new DomainException("Không thể sửa yêu cầu đã được phê duyệt.");
        Requestor = requestor.Trim();
        RequestingUnit = requestingUnit?.Trim();
        Deadline = deadline;
        ProcurementPurpose = procurementPurpose?.Trim();
        Touch(updatedBy);
    }

    public void Submit(string? updatedBy)
    {
        if (Status != PurchaseRequestStatus.NotSent)
            throw new DomainException($"Yêu cầu phải ở trạng thái Chưa gửi. Hiện tại: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException("Yêu cầu phải có ít nhất một dòng vật tư.");
        Status = PurchaseRequestStatus.Submitted;
        Touch(updatedBy);
    }

    public void Approve(string? updatedBy)
    {
        if (Status != PurchaseRequestStatus.Submitted)
            throw new DomainException($"Yêu cầu phải ở trạng thái Đã gửi. Hiện tại: {Status}.");
        Status = PurchaseRequestStatus.Approved;
        Touch(updatedBy);
    }

    public void Reject(string? updatedBy)
    {
        if (Status != PurchaseRequestStatus.Submitted)
            throw new DomainException($"Yêu cầu phải ở trạng thái Đã gửi. Hiện tại: {Status}.");
        Status = PurchaseRequestStatus.Rejected;
        Touch(updatedBy);
    }
}
