using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability;

public enum AggregationType { Pack, Unpack }

public class SerialAggregation : Entity
{
    public long AggregationID { get; private set; }
    public Guid? ChildSerialID { get; private set; }
    public string? ChildSSCC { get; private set; }
    public string ParentSSCC { get; private set; } = string.Empty;
    public AggregationType AggregationType { get; private set; }
    public DateTime AggregatedAt { get; private set; }
    public DateTime? DisaggregatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private SerialAggregation() { }

    public static SerialAggregation Pack(Guid serialId, string parentSSCC)
        => new()
        {
            ChildSerialID = serialId,
            ParentSSCC = parentSSCC.Trim().ToUpperInvariant(),
            AggregationType = AggregationType.Pack,
            AggregatedAt = DateTime.UtcNow,
            IsActive = true,
        };

    public static SerialAggregation PackSSCC(string childSSCC, string parentSSCC)
        => new()
        {
            ChildSSCC = childSSCC.Trim().ToUpperInvariant(),
            ParentSSCC = parentSSCC.Trim().ToUpperInvariant(),
            AggregationType = AggregationType.Pack,
            AggregatedAt = DateTime.UtcNow,
            IsActive = true,
        };

    public void Disaggregate()
    {
        DisaggregatedAt = DateTime.UtcNow;
        IsActive = false;
    }
}
