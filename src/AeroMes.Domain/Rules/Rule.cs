namespace AeroMes.Domain.Rules;

public class Rule
{
    public int RuleId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Priority { get; private set; } = 100;
    public string TriggerType { get; private set; } = string.Empty;
    public string TriggerConfig { get; private set; } = "{}";
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<RuleCondition> _conditions = [];
    private readonly List<RuleAction> _actions = [];
    public IReadOnlyList<RuleCondition> Conditions => _conditions;
    public IReadOnlyList<RuleAction> Actions => _actions;

    private Rule() { }

    public static Rule Create(string code, string name, string? description,
        int priority, string triggerType, string triggerConfig, string createdBy)
        => new()
        {
            Code = code, Name = name, Description = description,
            Priority = priority, TriggerType = triggerType,
            TriggerConfig = triggerConfig, CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };

    public void Update(string name, string? description, int priority,
        string triggerType, string triggerConfig)
    {
        Name = name; Description = description; Priority = priority;
        TriggerType = triggerType; TriggerConfig = triggerConfig;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle(bool active)
    {
        IsActive = active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetConditions(IEnumerable<RuleCondition> conditions)
    {
        _conditions.Clear();
        _conditions.AddRange(conditions);
    }

    public void SetActions(IEnumerable<RuleAction> actions)
    {
        _actions.Clear();
        _actions.AddRange(actions);
    }
}
