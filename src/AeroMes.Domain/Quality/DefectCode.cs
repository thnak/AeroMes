using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum DefectSeverityLevel { Minor, Major, Critical }

public class DefectCode : AuditableEntity
{
    public int DefectCodeID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string DefectName { get; private set; } = string.Empty;
    public string? DefectCategory { get; private set; }
    public string? Description { get; private set; }
    public DefectSeverityLevel SeverityLevel { get; private set; } = DefectSeverityLevel.Minor;
    public bool IsMajorDefault { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public bool IsRepairable { get; private set; } = false;

    private DefectCode() { }

    public static DefectCode Create(
        string code, string name, string? category = null,
        bool isRepairable = false, bool isMajorDefault = false,
        DefectSeverityLevel severityLevel = DefectSeverityLevel.Minor,
        string? description = null, string? createdBy = null)
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
            Description = description?.Trim(),
            IsRepairable = isRepairable,
            IsMajorDefault = isMajorDefault || severityLevel >= DefectSeverityLevel.Major,
            SeverityLevel = severityLevel,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, string? category, bool isActive, bool isRepairable, string? updatedBy,
        bool isMajorDefault = false,
        DefectSeverityLevel severityLevel = DefectSeverityLevel.Minor,
        string? description = null)
    {
        DefectName = name.Trim();
        DefectCategory = category;
        Description = description?.Trim();
        IsActive = isActive;
        IsRepairable = isRepairable;
        SeverityLevel = severityLevel;
        IsMajorDefault = isMajorDefault || severityLevel >= DefectSeverityLevel.Major;
        Touch(updatedBy);
    }
}
