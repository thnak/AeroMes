using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class FactoryWarehouseReceipt : AuditableEntity
{
    public int ReceiptId { get; private set; }
    public string VoucherNumber { get; private set; } = string.Empty;
    public FactoryReceiptType ReceiptType { get; private set; }
    public FactoryReceiptStatus Status { get; private set; } = FactoryReceiptStatus.Draft;
    public int? ReferenceRequestId { get; private set; }
    public string SupplierOrTransferringUnit { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    private readonly List<FactoryReceiptLine> _lines = [];
    public IReadOnlyList<FactoryReceiptLine> Lines => _lines.AsReadOnly();

    private FactoryWarehouseReceipt() { }

    public static FactoryWarehouseReceipt Create(
        string voucherNumber,
        FactoryReceiptType receiptType,
        int? referenceRequestId,
        string supplierOrTransferringUnit,
        string? notes,
        string? createdBy)
    {
        return new FactoryWarehouseReceipt
        {
            VoucherNumber = voucherNumber.Trim().ToUpperInvariant(),
            ReceiptType = receiptType,
            Status = FactoryReceiptStatus.Draft,
            ReferenceRequestId = referenceRequestId,
            SupplierOrTransferringUnit = supplierOrTransferringUnit.Trim(),
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public FactoryReceiptLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        int destinationWarehouseId,
        string? specificationCode)
    {
        if (Status != FactoryReceiptStatus.Draft)
            throw new DomainException($"Phiếu nhập '{VoucherNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (quantity <= 0)
            throw new DomainException("Số lượng nhập phải lớn hơn 0.");

        var line = FactoryReceiptLine.Create(ReceiptId, productCode, unitOfMeasure, quantity, destinationWarehouseId, specificationCode);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != FactoryReceiptStatus.Draft)
            throw new DomainException($"Phiếu nhập '{VoucherNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        FactoryReceiptType receiptType,
        int? referenceRequestId,
        string supplierOrTransferringUnit,
        string? notes,
        string? updatedBy)
    {
        if (Status != FactoryReceiptStatus.Draft)
            throw new DomainException($"Phiếu nhập '{VoucherNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        if (receiptType != ReceiptType)
        {
            ReceiptType = receiptType;
            _lines.Clear();
        }

        ReferenceRequestId = referenceRequestId;
        SupplierOrTransferringUnit = supplierOrTransferringUnit.Trim();
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Confirm(string? confirmedBy)
    {
        if (Status != FactoryReceiptStatus.Draft)
            throw new DomainException($"Phiếu nhập '{VoucherNumber}' phải ở trạng thái Nháp để xác nhận.");
        if (_lines.Count == 0)
            throw new DomainException($"Phiếu nhập '{VoucherNumber}' phải có ít nhất một dòng vật tư trước khi xác nhận.");
        Status = FactoryReceiptStatus.Confirmed;
        Touch(confirmedBy);
    }
}
