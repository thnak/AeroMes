using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class CycleCountPlan : AuditableEntity
{
    public int PlanId { get; private set; }
    public string PlanCode { get; private set; } = string.Empty;
    public CycleCountPlanType PlanType { get; private set; }
    public DateOnly ScheduledDate { get; private set; }
    public CycleCountPlanStatus Status { get; private set; } = CycleCountPlanStatus.Draft;
    public string? Notes { get; private set; }

    private readonly List<CycleCountLine> _lines = [];
    public IReadOnlyList<CycleCountLine> Lines => _lines;

    private CycleCountPlan() { }

    public static CycleCountPlan Create(
        string planCode,
        CycleCountPlanType planType,
        DateOnly scheduledDate,
        string? notes,
        string? createdBy)
        => new()
        {
            PlanCode = planCode,
            PlanType = planType,
            ScheduledDate = scheduledDate,
            Notes = notes,
            Status = CycleCountPlanStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

    public void AddLine(int binId, int locationId, string productCode, string lotNumber, decimal bookQty)
    {
        if (Status != CycleCountPlanStatus.Draft)
            throw new DomainException("Chỉ có thể thêm dòng kiểm kê khi kế hoạch ở trạng thái Nháp.");
        _lines.Add(CycleCountLine.Create(PlanId, binId, locationId, productCode, lotNumber, bookQty));
    }

    public void ClearLines()
    {
        if (Status != CycleCountPlanStatus.Draft)
            throw new DomainException("Chỉ có thể xóa dòng kiểm kê khi kế hoạch ở trạng thái Nháp.");
        _lines.Clear();
    }

    public void StartCount()
    {
        if (Status != CycleCountPlanStatus.Draft)
            throw new DomainException("Chỉ có thể bắt đầu kiểm kê từ trạng thái Nháp.");
        if (_lines.Count == 0)
            throw new DomainException("Cần ít nhất một dòng kiểm kê trước khi bắt đầu.");
        Status = CycleCountPlanStatus.InProgress;
        Touch(null);
    }

    public void RecordCount(long lineId, decimal countedQty, string? countedBy)
    {
        if (Status != CycleCountPlanStatus.InProgress)
            throw new DomainException("Chỉ có thể ghi số kiểm kê khi kế hoạch đang thực hiện.");
        var line = _lines.FirstOrDefault(l => l.LineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng kiểm kê #{lineId}.");
        line.RecordCount(countedQty, countedBy);
    }

    public void SubmitForApproval()
    {
        if (Status != CycleCountPlanStatus.InProgress)
            throw new DomainException("Chỉ có thể gửi phê duyệt khi kế hoạch đang thực hiện.");
        if (_lines.Any(l => l.Status == CycleCountLineStatus.Pending))
            throw new DomainException("Còn dòng kiểm kê chưa ghi số đếm. Không thể gửi phê duyệt.");
        Status = CycleCountPlanStatus.PendingApproval;
        Touch(null);
    }

    public void ApproveLine(long lineId)
    {
        if (Status != CycleCountPlanStatus.PendingApproval)
            throw new DomainException("Kế hoạch phải ở trạng thái Chờ phê duyệt.");
        var line = _lines.FirstOrDefault(l => l.LineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng kiểm kê #{lineId}.");
        line.Approve();
    }

    public void RejectLineForRecount(long lineId)
    {
        if (Status != CycleCountPlanStatus.PendingApproval)
            throw new DomainException("Kế hoạch phải ở trạng thái Chờ phê duyệt.");
        var line = _lines.FirstOrDefault(l => l.LineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng kiểm kê #{lineId}.");
        line.RejectForRecount();
    }

    public void CompletePlan(string? approvedBy)
    {
        if (Status != CycleCountPlanStatus.PendingApproval)
            throw new DomainException("Kế hoạch phải ở trạng thái Chờ phê duyệt để hoàn thành.");
        Status = CycleCountPlanStatus.Completed;
        Touch(approvedBy);
    }

    public void RevertToInProgress()
    {
        Status = CycleCountPlanStatus.InProgress;
    }

    public void CancelPlan(string? cancelledBy)
    {
        if (Status is CycleCountPlanStatus.Completed or CycleCountPlanStatus.Cancelled)
            throw new DomainException("Không thể hủy kế hoạch đã hoàn thành hoặc đã hủy.");
        SoftDelete(cancelledBy);
        Status = CycleCountPlanStatus.Cancelled;
    }

    public void UpdatePlan(CycleCountPlanType planType, DateOnly scheduledDate, string? notes, string? updatedBy)
    {
        if (Status != CycleCountPlanStatus.Draft)
            throw new DomainException("Chỉ có thể cập nhật kế hoạch khi ở trạng thái Nháp.");
        PlanType = planType;
        ScheduledDate = scheduledDate;
        Notes = notes;
        Touch(updatedBy);
    }
}
