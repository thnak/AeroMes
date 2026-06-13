using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot;

public class SignalMapping : AuditableEntity
{
    public int SignalID { get; private set; }
    public int AdapterID { get; private set; }
    public string TagKey { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string SourceAddress { get; private set; } = string.Empty;
    public double Scale { get; private set; } = 1.0;
    public double Offset { get; private set; }
    public double? QualityMin { get; private set; }
    public double? QualityMax { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public AdapterInstance? Adapter { get; private set; }

    private SignalMapping() { }

    public static SignalMapping Create(int adapterId, string tagKey, string displayName, string sourceAddress,
        double scale, double offset, double? qualityMin, double? qualityMax, string? createdBy)
        => new()
        {
            AdapterID = adapterId, TagKey = tagKey, DisplayName = displayName, SourceAddress = sourceAddress,
            Scale = scale, Offset = offset, QualityMin = qualityMin, QualityMax = qualityMax,
            CreatedBy = createdBy, CreatedAt = DateTime.UtcNow,
        };

    public void Update(string displayName, string sourceAddress, double scale, double offset,
        double? qualityMin, double? qualityMax, string updatedBy)
    {
        DisplayName = displayName; SourceAddress = sourceAddress;
        Scale = scale; Offset = offset; QualityMin = qualityMin; QualityMax = qualityMax;
        Touch(updatedBy);
    }

    public void Toggle(bool enabled, string updatedBy) { IsEnabled = enabled; Touch(updatedBy); }
}
