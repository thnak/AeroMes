using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class Operation : Entity
{
    public string OperationCode { get; private set; } = string.Empty;  // PK — e.g. CUT, WELD
    public string OperationName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Operation() { }

    public static Operation Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Operation code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Operation name is required.");

        return new Operation
        {
            OperationCode = code.Trim().ToUpperInvariant(),
            OperationName = name.Trim(),
            Description = description,
            IsActive = true,
        };
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Operation name is required.");
        OperationName = name.Trim();
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
