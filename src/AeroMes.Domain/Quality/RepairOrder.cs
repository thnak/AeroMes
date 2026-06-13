namespace AeroMes.Domain.Quality;

public class RepairOrder
{
    public int RepairOrderId { get; private set; }
    public string RepairOrderNo { get; private set; } = "";
    public string Status { get; private set; } = "DRAFT";  // DRAFT | IN_PROGRESS | COMPLETED | CANCELLED
    public string? Notes { get; private set; }
    public string CreatedBy { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private readonly List<RepairOrderEntry> _entries = [];
    private readonly List<RepairMaterialLine> _materialLines = [];

    public IReadOnlyCollection<RepairOrderEntry> Entries => _entries.AsReadOnly();
    public IReadOnlyCollection<RepairMaterialLine> MaterialLines => _materialLines.AsReadOnly();

    private RepairOrder() { }

    public static RepairOrder Create(string repairOrderNo, string createdBy, string? notes = null)
    {
        return new RepairOrder
        {
            RepairOrderNo = repairOrderNo,
            Status = "DRAFT",
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void AddEntry(int defectEntryId) =>
        _entries.Add(RepairOrderEntry.Create(0, defectEntryId));

    public void AddMaterialLine(int materialId, string materialCode, string materialName,
        decimal requiredQty, string unit) =>
        _materialLines.Add(RepairMaterialLine.Create(0, materialId, materialCode, materialName, requiredQty, unit));

    public void Start() => Status = "IN_PROGRESS";
    public void Complete() { Status = "COMPLETED"; CompletedAt = DateTimeOffset.UtcNow; }
    public void Cancel() => Status = "CANCELLED";
}

public class RepairOrderEntry
{
    public int RepairOrderEntryId { get; private set; }
    public int RepairOrderId { get; private set; }
    public int DefectEntryId { get; private set; }

    private RepairOrderEntry() { }

    internal static RepairOrderEntry Create(int repairOrderId, int defectEntryId) =>
        new() { RepairOrderId = repairOrderId, DefectEntryId = defectEntryId };
}

public class RepairMaterialLine
{
    public int RepairMaterialLineId { get; private set; }
    public int RepairOrderId { get; private set; }
    public int MaterialId { get; private set; }
    public string MaterialCode { get; private set; } = "";
    public string MaterialName { get; private set; } = "";
    public decimal RequiredQty { get; private set; }
    public decimal IssuedQty { get; private set; }
    public string Unit { get; private set; } = "";

    private RepairMaterialLine() { }

    internal static RepairMaterialLine Create(int repairOrderId, int materialId,
        string materialCode, string materialName, decimal requiredQty, string unit) =>
        new()
        {
            RepairOrderId = repairOrderId,
            MaterialId = materialId,
            MaterialCode = materialCode,
            MaterialName = materialName,
            RequiredQty = requiredQty,
            IssuedQty = 0,
            Unit = unit,
        };

    public void RecordIssued(decimal qty) => IssuedQty = qty;
}
