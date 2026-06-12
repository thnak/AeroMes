using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

/// <summary>Which products a mold can produce, with the standard cycle time per combo.</summary>
public class MoldProductMapping : Entity
{
    public int MappingId { get; private set; }
    public int MoldId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public double? CycleTimeSeconds { get; private set; }

    // EF navigation
    public Product? Product { get; private set; }

    private MoldProductMapping() { }

    internal static MoldProductMapping Create(
        int moldId, string productCode, bool isDefault, double? cycleTimeSeconds)
    {
        return new MoldProductMapping
        {
            MoldId = moldId,
            ProductCode = productCode,
            IsDefault = isDefault,
            CycleTimeSeconds = cycleTimeSeconds,
        };
    }

    internal void ClearDefault() => IsDefault = false;
}
