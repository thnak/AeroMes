using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum CriteriaType { Quantitative, Qualitative }
public enum CriteriaStatus { Active, Discontinued }

public class QualityCriteria : AuditableEntity
{
    public int CriteriaID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int? GroupID { get; private set; }
    public CriteriaType CriteriaType { get; private set; }
    public string? InspectionMethod { get; private set; }
    public string? MethodDescription { get; private set; }
    public CriteriaStatus Status { get; private set; } = CriteriaStatus.Active;

    private QualityCriteria() { }

    public static QualityCriteria Create(
        string code, string name, int? groupId,
        CriteriaType criteriaType, string? inspectionMethod,
        string? methodDescription, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã tiêu chí không được để trống.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên tiêu chí không được để trống.");
        return new QualityCriteria
        {
            Code = code.Trim(), Name = name.Trim(), GroupID = groupId,
            CriteriaType = criteriaType, InspectionMethod = inspectionMethod,
            MethodDescription = methodDescription, CreatedBy = createdBy
        };
    }

    public void Update(
        string name, int? groupId, CriteriaType criteriaType,
        string? inspectionMethod, string? methodDescription, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên tiêu chí không được để trống.");
        Name = name.Trim();
        GroupID = groupId;
        CriteriaType = criteriaType;
        InspectionMethod = inspectionMethod;
        MethodDescription = methodDescription;
        Touch(updatedBy);
    }

    public void Activate(string? updatedBy) { Status = CriteriaStatus.Active; Touch(updatedBy); }
    public void Discontinue(string? updatedBy) { Status = CriteriaStatus.Discontinued; Touch(updatedBy); }
}
