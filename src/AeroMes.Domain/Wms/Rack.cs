using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class Rack : AuditableEntity
{
    public int RackId { get; private set; }
    public int AisleId { get; private set; }
    public string RackCode { get; private set; } = string.Empty;
    public decimal? MaxWeightKg { get; private set; }

    // EF navigation
    public Aisle? Aisle { get; private set; }

    private Rack() { }

    public static Rack Create(int aisleId, string code, decimal? maxWeightKg = null, string? createdBy = null)
        => new()
        {
            AisleId = aisleId,
            RackCode = code.Trim().ToUpperInvariant(),
            MaxWeightKg = maxWeightKg,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
}
