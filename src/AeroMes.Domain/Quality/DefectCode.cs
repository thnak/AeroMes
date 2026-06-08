using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public class DefectCode : AuditableEntity
{
    public int DefectCodeID { get; private set; }
    public string Code { get; private set; } = string.Empty;          // e.g. ERR_SCRATCH
    public string DefectName { get; private set; } = string.Empty;
    public string? DefectCategory { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DefectCode() { }

    public static DefectCode Create(string code, string name, string? category = null, string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Defect code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Defect name is required.");

        return new DefectCode
        {
            Code = code.Trim().ToUpperInvariant(),
            DefectName = name.Trim(),
            DefectCategory = category,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
