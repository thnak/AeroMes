using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class Bin : AuditableEntity
{
    public int BinId { get; private set; }
    public int RackId { get; private set; }
    public string BinCode { get; private set; } = string.Empty;
    public string BinLevel { get; private set; } = string.Empty;
    public decimal? MaxQty { get; private set; }
    public BinType BinType { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF navigation
    public Rack? Rack { get; private set; }

    private Bin() { }

    public static Bin Create(
        int rackId,
        string code,
        string binLevel,
        BinType binType,
        decimal? maxQty = null,
        string? createdBy = null)
        => new()
        {
            RackId = rackId,
            BinCode = code.Trim().ToUpperInvariant(),
            BinLevel = binLevel.Trim().ToUpperInvariant(),
            BinType = binType,
            MaxQty = maxQty,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

    public void UpdateDetails(string binLevel, BinType binType, decimal? maxQty, string updatedBy)
    {
        BinLevel = binLevel.Trim().ToUpperInvariant();
        BinType = binType;
        MaxQty = maxQty;
        Touch(updatedBy);
    }

    public void Activate(string updatedBy) { IsActive = true; Touch(updatedBy); }
    public void Deactivate(string updatedBy) { IsActive = false; Touch(updatedBy); }
}
