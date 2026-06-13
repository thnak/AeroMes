using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum StatisticsSheetType { SingleProduct, MultiProduct, Disassembly }
public enum StatisticsSheetStatus { Draft, Submitted, Approved, Cancelled }

public class ProductionStatisticsSheet : AuditableEntity
{
    public int SheetId { get; private set; }
    public string SheetNumber { get; private set; } = string.Empty;    // PSS-2026-0001
    public StatisticsSheetType SheetType { get; private set; }
    public StatisticsSheetStatus Status { get; private set; } = StatisticsSheetStatus.Draft;
    public int? POID { get; private set; }
    public int? MPOId { get; private set; }
    public DateOnly ProductionDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<ProductionOutputLine> _outputLines = [];
    private readonly List<MaterialConsumptionLine> _materialLines = [];
    private readonly List<ByProductLine> _byProductLines = [];

    public IReadOnlyList<ProductionOutputLine> OutputLines => _outputLines.AsReadOnly();
    public IReadOnlyList<MaterialConsumptionLine> MaterialLines => _materialLines.AsReadOnly();
    public IReadOnlyList<ByProductLine> ByProductLines => _byProductLines.AsReadOnly();

    private ProductionStatisticsSheet() { }

    public static ProductionStatisticsSheet Create(
        string sheetNumber,
        StatisticsSheetType sheetType,
        int? poId,
        int? mpoId,
        DateOnly productionDate,
        string? notes,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(sheetNumber))
            throw new DomainException("Sheet number is required.");
        if (poId is null && mpoId is null)
            throw new DomainException("Must reference either a production order or a multi-product order.");

        return new ProductionStatisticsSheet
        {
            SheetNumber = sheetNumber.Trim().ToUpperInvariant(),
            SheetType = sheetType,
            Status = StatisticsSheetStatus.Draft,
            POID = poId,
            MPOId = mpoId,
            ProductionDate = productionDate,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public ProductionOutputLine AddOutputLine(
        string productCode, int plannedQty, int qualifiedQty, int defectiveQty, int? defectCodeId)
    {
        if (Status != StatisticsSheetStatus.Draft)
            throw new DomainException("Chỉ có thể cập nhật phiếu ở trạng thái Draft.");
        if (qualifiedQty < 0 || defectiveQty < 0)
            throw new DomainException("Số lượng không thể âm.");
        if (defectiveQty > 0 && defectCodeId is null)
            throw new DomainException("Cần chọn mã lỗi khi có số lượng NG.");

        var line = new ProductionOutputLine(
            SheetId, productCode.Trim().ToUpperInvariant(),
            plannedQty, qualifiedQty, defectiveQty, defectCodeId);
        _outputLines.Add(line);
        return line;
    }

    public MaterialConsumptionLine AddMaterialLine(
        string materialCode, decimal bomStandardQty, decimal actualUsedQty, string uomCode, string? varianceReason)
    {
        if (Status != StatisticsSheetStatus.Draft)
            throw new DomainException("Chỉ có thể cập nhật phiếu ở trạng thái Draft.");
        if (actualUsedQty < 0)
            throw new DomainException("Số lượng vật liệu sử dụng không thể âm.");

        var line = new MaterialConsumptionLine(
            SheetId, materialCode.Trim().ToUpperInvariant(),
            bomStandardQty, actualUsedQty, uomCode.Trim(), varianceReason?.Trim());
        _materialLines.Add(line);
        return line;
    }

    public ByProductLine AddByProductLine(string productCode, int qty, string uomCode, string? warehouseCode)
    {
        if (Status != StatisticsSheetStatus.Draft)
            throw new DomainException("Chỉ có thể cập nhật phiếu ở trạng thái Draft.");
        if (qty <= 0)
            throw new DomainException("Số lượng phụ phẩm phải lớn hơn 0.");

        var line = new ByProductLine(SheetId, productCode.Trim().ToUpperInvariant(), qty, uomCode.Trim(), warehouseCode?.Trim());
        _byProductLines.Add(line);
        return line;
    }

    public void Submit()
    {
        if (Status != StatisticsSheetStatus.Draft)
            throw new DomainException("Chỉ có thể nộp phiếu ở trạng thái Draft.");
        if (_outputLines.Count == 0)
            throw new DomainException("Phiếu phải có ít nhất một dòng sản lượng.");
        Status = StatisticsSheetStatus.Submitted;
    }

    public void Approve()
    {
        if (Status != StatisticsSheetStatus.Submitted)
            throw new DomainException("Chỉ có thể duyệt phiếu ở trạng thái Submitted.");
        Status = StatisticsSheetStatus.Approved;
    }

    public void Cancel()
    {
        if (Status is StatisticsSheetStatus.Approved or StatisticsSheetStatus.Cancelled)
            throw new DomainException("Không thể huỷ phiếu đã duyệt hoặc đã huỷ.");
        Status = StatisticsSheetStatus.Cancelled;
    }
}

public class ProductionOutputLine : Entity
{
    public int LineId { get; private set; }
    public int SheetId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int PlannedQty { get; private set; }
    public int QualifiedQty { get; private set; }
    public int DefectiveQty { get; private set; }
    public int? DefectCodeId { get; private set; }

    public int TotalProducedQty => QualifiedQty + DefectiveQty;

    private ProductionOutputLine() { }

    internal ProductionOutputLine(
        int sheetId, string productCode, int plannedQty,
        int qualifiedQty, int defectiveQty, int? defectCodeId)
    {
        SheetId = sheetId;
        ProductCode = productCode;
        PlannedQty = plannedQty;
        QualifiedQty = qualifiedQty;
        DefectiveQty = defectiveQty;
        DefectCodeId = defectCodeId;
    }
}

public class MaterialConsumptionLine : Entity
{
    public int LineId { get; private set; }
    public int SheetId { get; private set; }
    public string MaterialCode { get; private set; } = string.Empty;
    public decimal BomStandardQty { get; private set; }
    public decimal ActualUsedQty { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public string? VarianceReason { get; private set; }

    public decimal Variance => ActualUsedQty - BomStandardQty;

    private MaterialConsumptionLine() { }

    internal MaterialConsumptionLine(
        int sheetId, string materialCode, decimal bomStandardQty,
        decimal actualUsedQty, string uomCode, string? varianceReason)
    {
        SheetId = sheetId;
        MaterialCode = materialCode;
        BomStandardQty = bomStandardQty;
        ActualUsedQty = actualUsedQty;
        UoMCode = uomCode;
        VarianceReason = varianceReason;
    }
}

public class ByProductLine : Entity
{
    public int LineId { get; private set; }
    public int SheetId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int Qty { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public string? WarehouseCode { get; private set; }

    private ByProductLine() { }

    internal ByProductLine(int sheetId, string productCode, int qty, string uomCode, string? warehouseCode)
    {
        SheetId = sheetId;
        ProductCode = productCode;
        Qty = qty;
        UoMCode = uomCode;
        WarehouseCode = warehouseCode;
    }
}
