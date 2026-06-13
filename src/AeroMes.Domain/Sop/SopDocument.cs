namespace AeroMes.Domain.Sop;

public class SopDocument
{
    public int SopId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public int RoutingStepId { get; private set; }
    public string? ProductCode { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string Status { get; private set; } = "DRAFT";
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Notes { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<CheckItem> _items = [];
    public IReadOnlyList<CheckItem> Items => _items;

    private SopDocument() { }

    public static SopDocument Create(string code, string title, string version,
        int routingStepId, string? productCode, DateOnly effectiveFrom,
        string? notes, string createdBy)
        => new()
        {
            Code = code, Title = title, Version = version,
            RoutingStepId = routingStepId, ProductCode = productCode,
            EffectiveFrom = effectiveFrom, Status = "DRAFT",
            Notes = notes, CreatedBy = createdBy, CreatedAt = DateTime.UtcNow,
        };

    public void Activate(string approvedBy)
    {
        Status = "ACTIVE";
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Supersede() => Status = "SUPERSEDED";

    public void SetItems(IEnumerable<CheckItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }
}

public class CheckItem
{
    public int CheckItemId { get; private set; }
    public int SopId { get; private set; }
    public int Sequence { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string ItemText { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; } = true;
    public string CompletionMode { get; private set; } = "MANUAL";
    public string? AutoConfig { get; private set; }
    public decimal? SpecMin { get; private set; }
    public decimal? SpecMax { get; private set; }
    public string? Unit { get; private set; }
    public bool PhotoRequired { get; private set; }

    private CheckItem() { }

    public static CheckItem Create(int sopId, int sequence, string category, string itemText,
        bool isRequired, string completionMode, string? autoConfig,
        decimal? specMin, decimal? specMax, string? unit, bool photoRequired)
        => new()
        {
            SopId = sopId, Sequence = sequence, Category = category, ItemText = itemText,
            IsRequired = isRequired, CompletionMode = completionMode, AutoConfig = autoConfig,
            SpecMin = specMin, SpecMax = specMax, Unit = unit, PhotoRequired = photoRequired,
        };
}
