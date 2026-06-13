using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Operation : AuditableEntity
{
    public string OperationCode { get; private set; } = string.Empty;  // PK — e.g. CUT, WELD
    public string OperationName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public decimal? SAM_Minutes { get; private set; }
    public string? OperationCategory { get; private set; }

    private Operation() { }

    public static Operation Create(string code, string name, string? description = null, string? createdBy = null)
    {
        return new Operation
        {
            OperationCode = code.Trim().ToUpperInvariant(),
            OperationName = name.Trim(),
            Description = description,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, string? description)
    {
        OperationName = name.Trim();
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
