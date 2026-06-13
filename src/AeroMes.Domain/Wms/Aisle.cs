using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class Aisle : AuditableEntity
{
    public int AisleId { get; private set; }
    public int ZoneId { get; private set; }
    public string AisleCode { get; private set; } = string.Empty;
    public int PickSequence { get; private set; }

    // EF navigation
    public WarehouseZone? Zone { get; private set; }

    private Aisle() { }

    public static Aisle Create(int zoneId, string code, int pickSequence, string? createdBy = null)
        => new()
        {
            ZoneId = zoneId,
            AisleCode = code.Trim().ToUpperInvariant(),
            PickSequence = pickSequence,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

    public void UpdatePickSequence(int pickSequence, string updatedBy)
    {
        PickSequence = pickSequence;
        Touch(updatedBy);
    }
}
