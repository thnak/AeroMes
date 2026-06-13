using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum CriteriaGroupStatus { Active, Discontinued }

public class QualityCriteriaGroup : AuditableEntity
{
    public int GroupID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public CriteriaGroupStatus Status { get; private set; } = CriteriaGroupStatus.Active;

    private QualityCriteriaGroup() { }

    public static QualityCriteriaGroup Create(string code, string name, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã nhóm không được để trống.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên nhóm không được để trống.");
        return new QualityCriteriaGroup { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(), CreatedBy = createdBy };
    }

    public void UpdateName(string name, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên nhóm không được để trống.");
        Name = name.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(CriteriaGroupStatus status, string? updatedBy)
    {
        Status = status;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
