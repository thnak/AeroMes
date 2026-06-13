using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum ProductionPlanStatus { Draft, Confirmed, InProgress, Completed, Cancelled }
public enum PlanAllocationMethod { Auto, Manual }

public class ProductionPlanByOrder : AuditableEntity
{
    public int PlanId { get; private set; }
    public string PlanCode { get; private set; } = string.Empty;
    public int PoId { get; private set; }
    public PlanAllocationMethod AllocationMethod { get; private set; }
    public ProductionPlanStatus Status { get; private set; } = ProductionPlanStatus.Draft;
    public string? Notes { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    private readonly List<ProductionPlanOrderLine> _lines = [];
    public IReadOnlyList<ProductionPlanOrderLine> Lines => _lines.AsReadOnly();

    private ProductionPlanByOrder() { }

    public static ProductionPlanByOrder Create(
        string planCode, int poId, PlanAllocationMethod method, string? notes, string? createdBy)
    {
        return new ProductionPlanByOrder
        {
            PlanCode = planCode.Trim().ToUpperInvariant(),
            PoId = poId,
            AllocationMethod = method,
            Status = ProductionPlanStatus.Draft,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public ProductionPlanOrderLine AddLine(
        string productCode, decimal plannedQty,
        string? teamCode, DateTime? plannedStart, DateTime? plannedEnd)
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new DomainException("Chỉ có thể thêm dòng vào kế hoạch ở trạng thái Nháp.");
        if (plannedQty <= 0)
            throw new DomainException("Số lượng kế hoạch phải lớn hơn 0.");

        var line = ProductionPlanOrderLine.Create(PlanId, productCode, plannedQty, teamCode, plannedStart, plannedEnd);
        _lines.Add(line);
        return line;
    }

    public void UpdateLine(
        int planLineId, string? teamCode, DateTime? plannedStart, DateTime? plannedEnd, string? updatedBy)
    {
        if (Status == ProductionPlanStatus.Completed || Status == ProductionPlanStatus.Cancelled)
            throw new DomainException($"Không thể cập nhật dòng kế hoạch ở trạng thái {Status}.");

        var line = _lines.FirstOrDefault(l => l.PlanLineId == planLineId)
            ?? throw new DomainException($"Dòng kế hoạch '{planLineId}' không tồn tại.");
        line.UpdateAssignment(teamCode, plannedStart, plannedEnd);
        Touch(updatedBy);
    }

    public void Confirm(string? updatedBy)
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new DomainException($"Chỉ có thể xác nhận kế hoạch ở trạng thái Nháp. Hiện tại: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException("Kế hoạch phải có ít nhất một dòng sản phẩm.");

        Status = ProductionPlanStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        Touch(updatedBy);
    }

    public void Start(string? updatedBy)
    {
        if (Status != ProductionPlanStatus.Confirmed)
            throw new DomainException($"Kế hoạch phải ở trạng thái Đã xác nhận. Hiện tại: {Status}.");
        Status = ProductionPlanStatus.InProgress;
        Touch(updatedBy);
    }

    public void Complete(string? updatedBy)
    {
        if (Status != ProductionPlanStatus.InProgress)
            throw new DomainException($"Kế hoạch phải ở trạng thái Đang thực hiện. Hiện tại: {Status}.");
        Status = ProductionPlanStatus.Completed;
        Touch(updatedBy);
    }

    public void Cancel(string? updatedBy)
    {
        if (Status is ProductionPlanStatus.Completed or ProductionPlanStatus.Cancelled)
            throw new DomainException($"Không thể hủy kế hoạch ở trạng thái {Status}.");
        Status = ProductionPlanStatus.Cancelled;
        Touch(updatedBy);
    }

    public void RecordActualQty(int planLineId, decimal actualQty, string? updatedBy)
    {
        var line = _lines.FirstOrDefault(l => l.PlanLineId == planLineId)
            ?? throw new DomainException($"Dòng kế hoạch '{planLineId}' không tồn tại.");
        line.RecordActual(actualQty);
        Touch(updatedBy);
    }
}
