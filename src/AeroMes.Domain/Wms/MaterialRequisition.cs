using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class MaterialRequisition : AuditableEntity
{
    public int RequisitionId { get; private set; }
    public string RequisitionNumber { get; private set; } = string.Empty;
    public int? ProductionOrderId { get; private set; }
    public string RequesterUnit { get; private set; } = string.Empty;
    public DateTime RequestDate { get; private set; }
    public MaterialRequisitionStatus Status { get; private set; } = MaterialRequisitionStatus.Draft;
    public DateTime? SentAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MaterialRequisitionLine> _lines = [];
    public IReadOnlyList<MaterialRequisitionLine> Lines => _lines.AsReadOnly();

    private MaterialRequisition() { }

    public static MaterialRequisition Create(
        string requisitionNumber,
        int? productionOrderId,
        string requesterUnit,
        DateTime requestDate,
        string? notes,
        string? createdBy)
    {
        return new MaterialRequisition
        {
            RequisitionNumber = requisitionNumber.Trim().ToUpperInvariant(),
            ProductionOrderId = productionOrderId,
            RequesterUnit = requesterUnit.Trim(),
            RequestDate = requestDate,
            Status = MaterialRequisitionStatus.Draft,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public MaterialRequisitionLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int warehouseId,
        string? notes)
    {
        if (Status != MaterialRequisitionStatus.Draft)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (requestedQuantity <= 0)
            throw new DomainException("Số lượng yêu cầu phải lớn hơn 0.");

        var line = MaterialRequisitionLine.Create(RequisitionId, productCode, unitOfMeasure, requestedQuantity, warehouseId, notes);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != MaterialRequisitionStatus.Draft)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        int? productionOrderId,
        string requesterUnit,
        DateTime requestDate,
        string? notes,
        string? updatedBy)
    {
        if (Status != MaterialRequisitionStatus.Draft)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        ProductionOrderId = productionOrderId;
        RequesterUnit = requesterUnit.Trim();
        RequestDate = requestDate;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Send(string? sentBy)
    {
        if (Status != MaterialRequisitionStatus.Draft)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Nháp để gửi kho.");
        if (_lines.Count == 0)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải có ít nhất một dòng vật tư trước khi gửi.");
        Status = MaterialRequisitionStatus.Sent;
        SentAt = DateTime.UtcNow;
        Touch(sentBy);
    }

    public void Recall(string? recalledBy)
    {
        if (Status != MaterialRequisitionStatus.Sent)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Đã gửi để thu hồi.");
        Status = MaterialRequisitionStatus.Recalled;
        Touch(recalledBy);
    }

    public void Fulfill(IReadOnlyDictionary<int, decimal> actualQtyByLineId, string? fulfilledBy)
    {
        if (Status != MaterialRequisitionStatus.Sent)
            throw new DomainException($"Yêu cầu xuất '{RequisitionNumber}' phải ở trạng thái Đã gửi để thực hiện xuất kho.");

        foreach (var line in _lines)
        {
            if (actualQtyByLineId.TryGetValue(line.LineId, out var qty))
                line.SetActualIssuedQuantity(qty);
        }

        Status = MaterialRequisitionStatus.Fulfilled;
        Touch(fulfilledBy);
    }
}
