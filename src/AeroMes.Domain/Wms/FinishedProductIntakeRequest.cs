using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class FinishedProductIntakeRequest : AuditableEntity
{
    public int IntakeRequestId { get; private set; }
    public string RequestNumber { get; private set; } = string.Empty;
    public IntakeRequestPurpose IntakePurpose { get; private set; }
    public IntakeWarehouseType WarehouseType { get; private set; }
    public IntakeRequestStatus Status { get; private set; } = IntakeRequestStatus.Draft;
    public int? ProductionOrderId { get; private set; }
    public string RequesterUnit { get; private set; } = string.Empty;
    public DateTime RequestDate { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<IntakeRequestLine> _lines = [];
    public IReadOnlyList<IntakeRequestLine> Lines => _lines.AsReadOnly();

    private FinishedProductIntakeRequest() { }

    public static FinishedProductIntakeRequest Create(
        string requestNumber,
        IntakeRequestPurpose intakePurpose,
        IntakeWarehouseType warehouseType,
        int? productionOrderId,
        string requesterUnit,
        DateTime requestDate,
        string? notes,
        string? createdBy)
    {
        return new FinishedProductIntakeRequest
        {
            RequestNumber = requestNumber.Trim().ToUpperInvariant(),
            IntakePurpose = intakePurpose,
            WarehouseType = warehouseType,
            ProductionOrderId = productionOrderId,
            RequesterUnit = requesterUnit.Trim(),
            RequestDate = requestDate,
            Status = IntakeRequestStatus.Draft,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public IntakeRequestLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int warehouseId,
        bool isDefective,
        string? defectReason,
        string? notes)
    {
        if (Status != IntakeRequestStatus.Draft)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (requestedQuantity <= 0)
            throw new DomainException("Số lượng yêu cầu nhập phải lớn hơn 0.");

        if (isDefective && string.IsNullOrWhiteSpace(defectReason))
            throw new DomainException("Phải nhập lý do lỗi khi đánh dấu thành phẩm lỗi.");

        var line = IntakeRequestLine.Create(IntakeRequestId, productCode, unitOfMeasure, requestedQuantity, warehouseId, isDefective, defectReason, notes);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != IntakeRequestStatus.Draft)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        IntakeRequestPurpose intakePurpose,
        IntakeWarehouseType warehouseType,
        int? productionOrderId,
        string requesterUnit,
        DateTime requestDate,
        string? notes,
        string? updatedBy)
    {
        if (Status != IntakeRequestStatus.Draft)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        if (intakePurpose != IntakePurpose || warehouseType != WarehouseType)
            _lines.Clear();

        IntakePurpose = intakePurpose;
        WarehouseType = warehouseType;
        ProductionOrderId = productionOrderId;
        RequesterUnit = requesterUnit.Trim();
        RequestDate = requestDate;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Send(string? sentBy)
    {
        if (Status != IntakeRequestStatus.Draft)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Nháp để gửi kho.");
        if (_lines.Count == 0)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải có ít nhất một dòng trước khi gửi.");
        Status = IntakeRequestStatus.Sent;
        SentAt = DateTime.UtcNow;
        Touch(sentBy);
    }

    public void Recall(string? recalledBy)
    {
        if (Status != IntakeRequestStatus.Sent)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Đã gửi để thu hồi.");
        Status = IntakeRequestStatus.Recalled;
        Touch(recalledBy);
    }

    public void Receive(IReadOnlyDictionary<int, decimal> actualQtyByLineId, string? receivedBy)
    {
        if (Status != IntakeRequestStatus.Sent)
            throw new DomainException($"Yêu cầu nhập '{RequestNumber}' phải ở trạng thái Đã gửi để xác nhận nhập kho.");

        foreach (var line in _lines)
        {
            if (actualQtyByLineId.TryGetValue(line.LineId, out var qty))
                line.SetActualReceivedQuantity(qty);
        }

        Status = IntakeRequestStatus.Received;
        Touch(receivedBy);
    }
}
