using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class MaterialSupplyRequest : AuditableEntity
{
    public int RequestId { get; private set; }
    public string VoucherNumber { get; private set; } = string.Empty;
    public MaterialSupplyRequestType RequestType { get; private set; }
    public MaterialSupplyRequestStatus Status { get; private set; } = MaterialSupplyRequestStatus.Draft;
    public string RequesterUnit { get; private set; } = string.Empty;
    public DateTime? RequiredByDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MaterialSupplyRequestLine> _lines = [];
    public IReadOnlyList<MaterialSupplyRequestLine> Lines => _lines.AsReadOnly();

    private MaterialSupplyRequest() { }

    public static MaterialSupplyRequest Create(
        string voucherNumber,
        MaterialSupplyRequestType requestType,
        string requesterUnit,
        DateTime? requiredByDate,
        string? notes,
        string? createdBy)
    {
        return new MaterialSupplyRequest
        {
            VoucherNumber = voucherNumber.Trim().ToUpperInvariant(),
            RequestType = requestType,
            Status = MaterialSupplyRequestStatus.Draft,
            RequesterUnit = requesterUnit.Trim(),
            RequiredByDate = requiredByDate,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public MaterialSupplyRequestLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int? warehouseId,
        string? notes)
    {
        if (Status != MaterialSupplyRequestStatus.Draft)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (requestedQuantity <= 0)
            throw new DomainException("Số lượng yêu cầu phải lớn hơn 0.");

        var line = MaterialSupplyRequestLine.Create(RequestId, productCode, unitOfMeasure, requestedQuantity, warehouseId, notes);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != MaterialSupplyRequestStatus.Draft)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        MaterialSupplyRequestType requestType,
        string requesterUnit,
        DateTime? requiredByDate,
        string? notes,
        string? updatedBy)
    {
        if (Status != MaterialSupplyRequestStatus.Draft)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        if (requestType != RequestType)
        {
            RequestType = requestType;
            _lines.Clear();
        }

        RequesterUnit = requesterUnit.Trim();
        RequiredByDate = requiredByDate;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Submit(string? submittedBy)
    {
        if (Status != MaterialSupplyRequestStatus.Draft)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Nháp để gửi duyệt.");
        if (_lines.Count == 0)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải có ít nhất một dòng vật tư trước khi gửi duyệt.");
        Status = MaterialSupplyRequestStatus.Submitted;
        Touch(submittedBy);
    }

    public void Approve(string? approvedBy)
    {
        if (Status != MaterialSupplyRequestStatus.Submitted)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Chờ duyệt để phê duyệt.");
        Status = MaterialSupplyRequestStatus.Approved;
        Touch(approvedBy);
    }

    public void Reject(string? rejectedBy)
    {
        if (Status != MaterialSupplyRequestStatus.Submitted)
            throw new DomainException($"Phiếu đề nghị '{VoucherNumber}' phải ở trạng thái Chờ duyệt để từ chối.");
        Status = MaterialSupplyRequestStatus.Rejected;
        Touch(rejectedBy);
    }
}
