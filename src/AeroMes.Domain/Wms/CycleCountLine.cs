using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class CycleCountLine : Entity
{
    public long LineId { get; private set; }
    public int PlanId { get; private set; }
    public int BinId { get; private set; }
    public int LocationId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal BookQty { get; private set; }
    public decimal? CountedQty { get; private set; }
    public string? CountedBy { get; private set; }
    public DateTime? CountedAt { get; private set; }
    public CycleCountLineStatus Status { get; private set; } = CycleCountLineStatus.Pending;

    public decimal? VarianceQty => CountedQty.HasValue ? CountedQty.Value - BookQty : null;
    public decimal? VariancePct => CountedQty.HasValue && BookQty != 0
        ? Math.Round((CountedQty.Value - BookQty) / BookQty * 100, 2)
        : null;

    private CycleCountLine() { }

    internal static CycleCountLine Create(
        int planId, int binId, int locationId,
        string productCode, string lotNumber, decimal bookQty)
        => new()
        {
            PlanId = planId,
            BinId = binId,
            LocationId = locationId,
            ProductCode = productCode,
            LotNumber = lotNumber,
            BookQty = bookQty,
            Status = CycleCountLineStatus.Pending,
        };

    internal void RecordCount(decimal countedQty, string? countedBy)
    {
        if (Status is not (CycleCountLineStatus.Pending or CycleCountLineStatus.Rejected))
            throw new DomainException($"Dòng kiểm kê đang ở trạng thái {Status}, không thể ghi số đếm.");
        if (countedQty < 0)
            throw new DomainException("Số lượng kiểm kê không được âm.");
        CountedQty = countedQty;
        CountedBy = countedBy;
        CountedAt = DateTime.UtcNow;
        Status = CycleCountLineStatus.Counted;
    }

    internal void Approve()
    {
        if (Status != CycleCountLineStatus.Counted)
            throw new DomainException("Dòng kiểm kê phải ở trạng thái Đã đếm để phê duyệt.");
        Status = CycleCountLineStatus.Approved;
    }

    internal void RejectForRecount()
    {
        if (Status != CycleCountLineStatus.Counted)
            throw new DomainException("Dòng kiểm kê phải ở trạng thái Đã đếm để từ chối.");
        CountedQty = null;
        CountedBy = null;
        CountedAt = null;
        Status = CycleCountLineStatus.Pending;
    }
}
