using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Routing : AuditableEntity
{
    public int RoutingID { get; private set; }
    public string RoutingCode { get; private set; } = string.Empty;
    public string RoutingName { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; } = true;
    public bool IsActive { get; private set; } = true;

    // backing field name must match what EF is told in OnModelCreating
    private readonly List<RoutingStep> _steps = [];
    public IReadOnlyCollection<RoutingStep> Steps => _steps.AsReadOnly();

    private Routing() { }

    public static Routing Create(
        string code,
        string name,
        string productCode,
        bool isDefault = true,
        string? createdBy = null)
    {
        return new Routing
        {
            RoutingCode = code.Trim().ToUpperInvariant(),
            RoutingName = name.Trim(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            IsDefault = isDefault,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, bool isDefault, string updatedBy)
    {
        RoutingName = name.Trim();
        IsDefault = isDefault;
        Touch(updatedBy);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
