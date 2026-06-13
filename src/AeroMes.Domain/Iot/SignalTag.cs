using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Iot;

public class SignalTag : Entity
{
    public int TagId { get; private set; }
    public string Key { get; private set; } = "";           // unique, e.g. spindle_rpm
    public string DisplayName { get; private set; } = "";
    public string Category { get; private set; } = "";      // MOTION|ELECTRICAL|THERMAL|COUNTER|STATUS|CUSTOM
    public string DataType { get; private set; } = "";      // FLOAT|INT|BOOL|STRING
    public string? DefaultUnit { get; private set; }
    public decimal? TypicalMin { get; private set; }
    public decimal? TypicalMax { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystemDefined { get; private set; }

    private SignalTag() { }

    public static SignalTag Create(string key, string displayName, string category, string dataType,
        string? defaultUnit, decimal? typicalMin, decimal? typicalMax, string? description, bool isSystemDefined = false)
        => new()
        {
            Key = key, DisplayName = displayName, Category = category, DataType = dataType,
            DefaultUnit = defaultUnit, TypicalMin = typicalMin, TypicalMax = typicalMax,
            Description = description, IsSystemDefined = isSystemDefined,
        };

    public void Update(string displayName, string category, string dataType,
        string? defaultUnit, decimal? typicalMin, decimal? typicalMax, string? description)
    {
        if (IsSystemDefined)
            throw new DomainException("System-defined tags cannot be modified.");
        DisplayName = displayName; Category = category; DataType = dataType;
        DefaultUnit = defaultUnit; TypicalMin = typicalMin; TypicalMax = typicalMax;
        Description = description;
    }

    public void GuardDelete()
    {
        if (IsSystemDefined)
            throw new DomainException("System-defined tags cannot be deleted.");
    }
}
