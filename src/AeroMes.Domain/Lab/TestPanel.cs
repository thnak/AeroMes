namespace AeroMes.Domain.Lab;

public class TestPanel
{
    public int PanelId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<TestPanelItem> _items = [];
    public IReadOnlyList<TestPanelItem> Items => _items;

    private TestPanel() { }

    public static TestPanel Create(string code, string name, string? productCode)
        => new()
        {
            Code = code, Name = name, ProductCode = productCode,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };

    public void Update(string name, string? productCode)
    {
        Name = name; ProductCode = productCode; UpdatedAt = DateTime.UtcNow;
    }

    public void SetItems(IEnumerable<TestPanelItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    public void Toggle(bool active) { IsActive = active; UpdatedAt = DateTime.UtcNow; }
}

public class TestPanelItem
{
    public int PanelItemId { get; private set; }
    public int PanelId { get; private set; }
    public int TestMethodId { get; private set; }
    public int Sequence { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public decimal? SpecOverrideMin { get; private set; }
    public decimal? SpecOverrideMax { get; private set; }

    private TestPanelItem() { }

    public static TestPanelItem Create(int panelId, int testMethodId, int sequence,
        bool isRequired = true, decimal? specOverrideMin = null, decimal? specOverrideMax = null)
        => new()
        {
            PanelId = panelId, TestMethodId = testMethodId, Sequence = sequence,
            IsRequired = isRequired, SpecOverrideMin = specOverrideMin, SpecOverrideMax = specOverrideMax,
        };
}
