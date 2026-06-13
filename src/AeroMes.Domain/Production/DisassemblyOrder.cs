using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum DisassemblyOrderType { Manual, FromPurchaseOrder, FromMPS }
public enum DisassemblyOrderStatus { NotStarted, InProgress, Completed, Paused, Canceled }

public class DisassemblyOrder : Entity
{
    private readonly List<DisassemblyRecoveredLine> _recoveredLines = [];

    public int DisassemblyOrderID { get; private set; }
    public string OrderCode { get; private set; } = string.Empty;
    public DisassemblyOrderType OrderType { get; private set; }
    public int? PurchaseOrderID { get; private set; }
    public string SourceProductCode { get; private set; } = string.Empty;
    public int DisassemblyBomId { get; private set; }
    public decimal SourceQty { get; private set; }
    public DisassemblyOrderStatus Status { get; private set; } = DisassemblyOrderStatus.NotStarted;
    public DateTime? Deadline { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyList<DisassemblyRecoveredLine> RecoveredLines => _recoveredLines.AsReadOnly();

    private DisassemblyOrder() { }

    public static DisassemblyOrder Create(
        string orderCode,
        DisassemblyOrderType orderType,
        string sourceProductCode,
        int disassemblyBomId,
        decimal sourceQty,
        IEnumerable<(string ProductCode, decimal ExpectedQty)> recoveredLines,
        int? purchaseOrderId = null,
        DateTime? deadline = null,
        string? notes = null)
    {
        if (sourceQty <= 0) throw new DomainException("SourceQty must be > 0.");
        var order = new DisassemblyOrder
        {
            OrderCode = orderCode.Trim().ToUpperInvariant(),
            OrderType = orderType,
            SourceProductCode = sourceProductCode.Trim().ToUpperInvariant(),
            DisassemblyBomId = disassemblyBomId,
            SourceQty = sourceQty,
            PurchaseOrderID = purchaseOrderId,
            Deadline = deadline,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var (pc, qty) in recoveredLines)
            order._recoveredLines.Add(new DisassemblyRecoveredLine
            {
                ProductCode = pc.Trim().ToUpperInvariant(),
                ExpectedQty = qty,
            });
        return order;
    }

    public void Start()
    {
        if (Status != DisassemblyOrderStatus.NotStarted)
            throw new DomainException("Only NotStarted orders can be started.");
        Status = DisassemblyOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != DisassemblyOrderStatus.InProgress)
            throw new DomainException("Only InProgress orders can be paused.");
        Status = DisassemblyOrderStatus.Paused;
    }

    public void Resume()
    {
        if (Status != DisassemblyOrderStatus.Paused)
            throw new DomainException("Only Paused orders can be resumed.");
        Status = DisassemblyOrderStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != DisassemblyOrderStatus.InProgress)
            throw new DomainException("Only InProgress orders can be completed.");
        Status = DisassemblyOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is DisassemblyOrderStatus.Completed or DisassemblyOrderStatus.Canceled)
            throw new DomainException("Cannot cancel a Completed or already Canceled order.");
        Status = DisassemblyOrderStatus.Canceled;
    }

    public void RecordActualQty(string productCode, decimal actualQty)
    {
        if (Status != DisassemblyOrderStatus.InProgress)
            throw new DomainException("Order must be InProgress to record recovered quantities.");
        var line = _recoveredLines.FirstOrDefault(l => l.ProductCode == productCode.Trim().ToUpperInvariant());
        if (line is null) throw new DomainException($"No recovered line for product '{productCode}'.");
        line.ActualQty = actualQty;
    }
}

public class DisassemblyRecoveredLine
{
    public int LineID { get; set; }
    public int DisassemblyOrderID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public decimal ExpectedQty { get; set; }
    public decimal ActualQty { get; set; }
}
