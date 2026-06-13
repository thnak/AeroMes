using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class MaterialTransferSlip : AuditableEntity
{
    public int SlipId { get; private set; }
    public string VoucherNumber { get; private set; } = string.Empty;
    public MaterialTransferType TransferType { get; private set; }
    public MaterialTransferStatus Status { get; private set; } = MaterialTransferStatus.Draft;
    public int? ReferenceRequestId { get; private set; }
    public int SourceWarehouseId { get; private set; }
    public int DestinationWarehouseId { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MaterialTransferLine> _lines = [];
    public IReadOnlyList<MaterialTransferLine> Lines => _lines.AsReadOnly();

    private MaterialTransferSlip() { }

    public static MaterialTransferSlip Create(
        string voucherNumber,
        MaterialTransferType transferType,
        int? referenceRequestId,
        int sourceWarehouseId,
        int destinationWarehouseId,
        string? notes,
        string? createdBy)
    {
        if (sourceWarehouseId == destinationWarehouseId)
            throw new DomainException("Kho nguồn và kho đích không được trùng nhau.");

        return new MaterialTransferSlip
        {
            VoucherNumber = voucherNumber.Trim().ToUpperInvariant(),
            TransferType = transferType,
            Status = MaterialTransferStatus.Draft,
            ReferenceRequestId = referenceRequestId,
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public MaterialTransferLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        string? specificationCode)
    {
        if (Status != MaterialTransferStatus.Draft)
            throw new DomainException($"Phiếu điều chuyển '{VoucherNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (quantity <= 0)
            throw new DomainException("Số lượng điều chuyển phải lớn hơn 0.");

        var line = MaterialTransferLine.Create(SlipId, productCode, unitOfMeasure, quantity, specificationCode);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != MaterialTransferStatus.Draft)
            throw new DomainException($"Phiếu điều chuyển '{VoucherNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        MaterialTransferType transferType,
        int? referenceRequestId,
        int sourceWarehouseId,
        int destinationWarehouseId,
        string? notes,
        string? updatedBy)
    {
        if (Status != MaterialTransferStatus.Draft)
            throw new DomainException($"Phiếu điều chuyển '{VoucherNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        if (sourceWarehouseId == destinationWarehouseId)
            throw new DomainException("Kho nguồn và kho đích không được trùng nhau.");

        TransferType = transferType;
        ReferenceRequestId = referenceRequestId;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Confirm(string? confirmedBy)
    {
        if (Status != MaterialTransferStatus.Draft)
            throw new DomainException($"Phiếu điều chuyển '{VoucherNumber}' phải ở trạng thái Nháp để xác nhận.");
        if (_lines.Count == 0)
            throw new DomainException($"Phiếu điều chuyển '{VoucherNumber}' phải có ít nhất một dòng vật tư trước khi xác nhận.");
        Status = MaterialTransferStatus.Confirmed;
        Touch(confirmedBy);
    }
}
