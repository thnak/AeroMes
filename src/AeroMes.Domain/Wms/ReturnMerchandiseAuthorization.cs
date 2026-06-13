using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class ReturnMerchandiseAuthorization : AuditableEntity
{
    public int RmaId { get; private set; }
    public string RmaCode { get; private set; } = string.Empty;
    public ReturnDirection ReturnDirection { get; private set; }
    public string? SourceDocumentType { get; private set; }
    public int? SourceDocumentId { get; private set; }
    public string ReturnReason { get; private set; } = string.Empty;
    public RmaStatus Status { get; private set; } = RmaStatus.Draft;
    public string? AuthorizedBy { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }

    private readonly List<RmaLine> _lines = [];
    public IReadOnlyList<RmaLine> Lines => _lines.AsReadOnly();

    private ReturnMerchandiseAuthorization() { }

    public static ReturnMerchandiseAuthorization Create(
        string rmaCode,
        ReturnDirection direction,
        string? sourceDocumentType,
        int? sourceDocumentId,
        string returnReason,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(returnReason))
            throw new DomainException("Lý do trả hàng không được để trống.");

        return new ReturnMerchandiseAuthorization
        {
            RmaCode = rmaCode.Trim().ToUpperInvariant(),
            ReturnDirection = direction,
            SourceDocumentType = sourceDocumentType?.Trim(),
            SourceDocumentId = sourceDocumentId,
            ReturnReason = returnReason.Trim(),
            Status = RmaStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public RmaLine AddLine(string productCode, string lotNumber, decimal returnQty)
    {
        if (Status != RmaStatus.Draft)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Draft để thêm dòng.");

        var line = RmaLine.Create(RmaId, productCode, lotNumber, returnQty);
        _lines.Add(line);
        return line;
    }

    public void RemoveLine(int lineId)
    {
        if (Status != RmaStatus.Draft)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Draft để xóa dòng.");

        var line = _lines.FirstOrDefault(l => l.RmaLineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng RMA #{lineId}.");

        _lines.Remove(line);
    }

    public void Authorize(string authorizedBy)
    {
        if (Status != RmaStatus.Draft)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Draft để phê duyệt. Hiện tại: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException($"RMA '{RmaCode}' phải có ít nhất một dòng trước khi phê duyệt.");

        Status = RmaStatus.Authorized;
        AuthorizedBy = authorizedBy;
        AuthorizedAt = DateTime.UtcNow;
        Touch(authorizedBy);
    }

    public void MarkReceived(string receivedBy, IReadOnlyList<(int LineId, decimal ReceivedQty)> lineReceipts)
    {
        if (Status != RmaStatus.Authorized)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Authorized để nhận hàng. Hiện tại: {Status}.");

        foreach (var (lineId, qty) in lineReceipts)
        {
            var line = _lines.FirstOrDefault(l => l.RmaLineId == lineId)
                ?? throw new DomainException($"Không tìm thấy dòng RMA #{lineId}.");
            line.RecordReceived(qty);
        }

        Status = RmaStatus.Received;
        Touch(receivedBy);
    }

    public void DisposeLine(int lineId, RmaDisposition disposition, long? movementId)
    {
        if (Status != RmaStatus.Received && Status != RmaStatus.Dispositioned)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Received hoặc Dispositioned để xử lý dòng. Hiện tại: {Status}.");

        var line = _lines.FirstOrDefault(l => l.RmaLineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng RMA #{lineId}.");

        line.Dispose(disposition, movementId);

        if (_lines.All(l => l.Disposition.HasValue))
            Status = RmaStatus.Dispositioned;
    }

    public void Close(string closedBy)
    {
        if (Status != RmaStatus.Dispositioned)
            throw new DomainException($"RMA '{RmaCode}' phải ở trạng thái Dispositioned để đóng. Hiện tại: {Status}.");

        Status = RmaStatus.Closed;
        Touch(closedBy);
    }

    public void Cancel(string cancelledBy)
    {
        if (Status is RmaStatus.Received or RmaStatus.Dispositioned or RmaStatus.Closed)
            throw new DomainException($"RMA '{RmaCode}' không thể hủy ở trạng thái {Status}.");

        Status = RmaStatus.Cancelled;
        SoftDelete(cancelledBy);
    }
}
