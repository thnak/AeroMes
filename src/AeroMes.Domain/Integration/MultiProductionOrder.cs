using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Integration;

public enum MultiProductionOrderType
{
    ManualEntry,
    FromPurchaseOrder,
    FromMasterPlan,
    FromDetailedPlan,
    FromOtherOrder,
    FromRepairOrder
}

public enum MultiProductionOrderStatus { Draft, Released, Running, Completed, Cancelled }

public class MultiProductionOrder : AuditableEntity
{
    public int MPOId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;   // MPO-2026-0001
    public MultiProductionOrderType OrderType { get; private set; }
    public string? SourceReference { get; private set; }              // source PO/plan number
    public DateTime? PlannedStart { get; private set; }
    public DateTime? PlannedEnd { get; private set; }
    public MultiProductionOrderStatus Status { get; private set; } = MultiProductionOrderStatus.Draft;
    public byte Priority { get; private set; } = 5;
    public string? ProductionUnit { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MultiProductionOrderLine> _lines = [];
    public IReadOnlyList<MultiProductionOrderLine> Lines => _lines.AsReadOnly();

    private MultiProductionOrder() { }

    public static MultiProductionOrder Create(
        string orderNumber,
        MultiProductionOrderType orderType,
        string? sourceReference,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        byte priority,
        string? productionUnit,
        string? notes,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new DomainException("Order number is required.");

        return new MultiProductionOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            OrderType = orderType,
            SourceReference = sourceReference?.Trim(),
            PlannedStart = plannedStart,
            PlannedEnd = plannedEnd,
            Priority = priority,
            ProductionUnit = productionUnit?.Trim(),
            Notes = notes?.Trim(),
            Status = MultiProductionOrderStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public MultiProductionOrderLine AddLine(
        string productCode, int plannedQty, string uomCode, string? bomVersion)
    {
        if (Status != MultiProductionOrderStatus.Draft)
            throw new DomainException("Chỉ có thể thêm sản phẩm khi lệnh ở trạng thái Draft.");
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("Mã sản phẩm không được để trống.");
        if (plannedQty <= 0)
            throw new DomainException($"Số lượng kế hoạch phải lớn hơn 0. Nhận: {plannedQty}.");
        if (_lines.Any(l => l.ProductCode == productCode.Trim().ToUpperInvariant()))
            throw new DomainException($"Sản phẩm '{productCode}' đã tồn tại trong lệnh sản xuất.");

        var line = new MultiProductionOrderLine(
            MPOId, _lines.Count + 1,
            productCode.Trim().ToUpperInvariant(),
            plannedQty, uomCode.Trim(), bomVersion?.Trim());
        _lines.Add(line);
        return line;
    }

    public void Release()
    {
        if (Status != MultiProductionOrderStatus.Draft)
            throw new DomainException("Chỉ có thể phát hành lệnh ở trạng thái Draft.");
        if (_lines.Count == 0)
            throw new DomainException("Lệnh sản xuất phải có ít nhất một sản phẩm.");
        Status = MultiProductionOrderStatus.Released;
    }

    public void Start()
    {
        if (Status != MultiProductionOrderStatus.Released)
            throw new DomainException("Chỉ có thể bắt đầu lệnh ở trạng thái Released.");
        Status = MultiProductionOrderStatus.Running;
    }

    public void Complete()
    {
        if (Status != MultiProductionOrderStatus.Running)
            throw new DomainException("Chỉ có thể hoàn thành lệnh ở trạng thái Running.");
        Status = MultiProductionOrderStatus.Completed;
    }

    public void Cancel(string reason)
    {
        if (Status is MultiProductionOrderStatus.Completed or MultiProductionOrderStatus.Cancelled)
            throw new DomainException("Không thể huỷ lệnh sản xuất đã hoàn thành hoặc đã huỷ.");
        Status = MultiProductionOrderStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"[Huỷ: {reason}]" : $"{Notes} [Huỷ: {reason}]";
    }
}

public class MultiProductionOrderLine : Entity
{
    public int LineId { get; private set; }
    public int MPOId { get; private set; }
    public int LineNo { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int PlannedQty { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public string? BomVersion { get; private set; }
    public int ActualQtyOK { get; private set; }
    public int ActualQtyNG { get; private set; }

    // Navigation
    public MultiProductionOrder? Order { get; private set; }

    private MultiProductionOrderLine() { }

    internal MultiProductionOrderLine(
        int mpoId, int lineNo, string productCode,
        int plannedQty, string uomCode, string? bomVersion)
    {
        MPOId = mpoId;
        LineNo = lineNo;
        ProductCode = productCode;
        PlannedQty = plannedQty;
        UoMCode = uomCode;
        BomVersion = bomVersion;
    }

    public void UpdateActuals(int qtyOk, int qtyNg)
    {
        if (qtyOk < 0 || qtyNg < 0)
            throw new DomainException("Số lượng thực tế không thể âm.");
        ActualQtyOK = qtyOk;
        ActualQtyNG = qtyNg;
    }
}
