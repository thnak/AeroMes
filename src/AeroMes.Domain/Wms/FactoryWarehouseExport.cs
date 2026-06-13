using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class FactoryWarehouseExport : AuditableEntity
{
    public int ExportId { get; private set; }
    public string VoucherNumber { get; private set; } = string.Empty;
    public FactoryExportType ExportType { get; private set; }
    public FactoryExportStatus Status { get; private set; } = FactoryExportStatus.Draft;
    public int? ReferenceRequestId { get; private set; }
    public string ReceiverOrReceivingUnit { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    private readonly List<FactoryExportLine> _lines = [];
    public IReadOnlyList<FactoryExportLine> Lines => _lines.AsReadOnly();

    private FactoryWarehouseExport() { }

    public static FactoryWarehouseExport Create(
        string voucherNumber,
        FactoryExportType exportType,
        int? referenceRequestId,
        string receiverOrReceivingUnit,
        string? notes,
        string? createdBy)
    {
        return new FactoryWarehouseExport
        {
            VoucherNumber = voucherNumber.Trim().ToUpperInvariant(),
            ExportType = exportType,
            Status = FactoryExportStatus.Draft,
            ReferenceRequestId = referenceRequestId,
            ReceiverOrReceivingUnit = receiverOrReceivingUnit.Trim(),
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public FactoryExportLine AddLine(
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        int sourceWarehouseId,
        string? specificationCode)
    {
        if (Status != FactoryExportStatus.Draft)
            throw new DomainException($"Phiếu xuất '{VoucherNumber}' phải ở trạng thái Nháp để thêm dòng.");

        if (quantity <= 0)
            throw new DomainException("Số lượng xuất phải lớn hơn 0.");

        var line = FactoryExportLine.Create(ExportId, productCode, unitOfMeasure, quantity, sourceWarehouseId, specificationCode);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        if (Status != FactoryExportStatus.Draft)
            throw new DomainException($"Phiếu xuất '{VoucherNumber}' phải ở trạng thái Nháp để xóa dòng.");
        _lines.Clear();
    }

    public void UpdateHeader(
        FactoryExportType exportType,
        int? referenceRequestId,
        string receiverOrReceivingUnit,
        string? notes,
        string? updatedBy)
    {
        if (Status != FactoryExportStatus.Draft)
            throw new DomainException($"Phiếu xuất '{VoucherNumber}' phải ở trạng thái Nháp để chỉnh sửa.");

        if (exportType != ExportType)
        {
            ExportType = exportType;
            _lines.Clear();
        }

        ReferenceRequestId = referenceRequestId;
        ReceiverOrReceivingUnit = receiverOrReceivingUnit.Trim();
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Confirm(string? confirmedBy)
    {
        if (Status != FactoryExportStatus.Draft)
            throw new DomainException($"Phiếu xuất '{VoucherNumber}' phải ở trạng thái Nháp để xác nhận.");
        if (_lines.Count == 0)
            throw new DomainException($"Phiếu xuất '{VoucherNumber}' phải có ít nhất một dòng vật tư trước khi xác nhận.");
        Status = FactoryExportStatus.Confirmed;
        Touch(confirmedBy);
    }
}
