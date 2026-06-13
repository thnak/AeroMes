using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum PackagingOrderStatus { Draft, InProgress, Completed }

public class PackagingOrder : Entity
{
    private readonly List<PackagingLabel> _labels = [];

    public int PackagingOrderID { get; private set; }
    public int WOID { get; private set; }
    public int PackagingBomID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string IdentificationCode { get; private set; } = string.Empty;
    public decimal PlannedQty { get; private set; }
    public decimal PackagedQty { get; private set; }
    public PackagingOrderStatus Status { get; private set; } = PackagingOrderStatus.Draft;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyList<PackagingLabel> Labels => _labels.AsReadOnly();

    private PackagingOrder() { }

    public static PackagingOrder Create(int woid, int packagingBomId, string productCode, decimal plannedQty, string identificationCode, string? notes = null)
    {
        if (plannedQty <= 0) throw new DomainException("PlannedQty must be > 0.");
        return new PackagingOrder
        {
            WOID = woid,
            PackagingBomID = packagingBomId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            IdentificationCode = identificationCode,
            PlannedQty = plannedQty,
            Notes = notes,
            Status = PackagingOrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Start()
    {
        if (Status != PackagingOrderStatus.Draft)
            throw new DomainException("Only Draft orders can be started.");
        Status = PackagingOrderStatus.InProgress;
    }

    public void RecordPackaged(decimal qty)
    {
        if (Status != PackagingOrderStatus.InProgress)
            throw new DomainException("Order must be InProgress to record packaged quantity.");
        if (qty <= 0) throw new DomainException("Quantity must be > 0.");
        PackagedQty += qty;
    }

    public void Complete()
    {
        if (Status != PackagingOrderStatus.InProgress)
            throw new DomainException("Only InProgress orders can be completed.");
        Status = PackagingOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public PackagingLabel AddLabel(string labelData)
    {
        var label = new PackagingLabel
        {
            PackagingOrderID = PackagingOrderID,
            LabelData = labelData,
        };
        _labels.Add(label);
        return label;
    }
}

public class PackagingLabel
{
    public int LabelID { get; set; }
    public int PackagingOrderID { get; set; }
    public string LabelData { get; set; } = string.Empty;
    public DateTime? PrintedAt { get; set; }
}
