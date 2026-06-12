using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class EmployeeSkill : Entity
{
    public int EmployeeSkillId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string OperationCode { get; private set; } = string.Empty;
    public int CertificationLevel { get; private set; }
    public DateOnly CertifiedAt { get; private set; }
    public DateOnly? ExpiresAt { get; private set; }

    public Operation? Operation { get; private set; }

    private EmployeeSkill() { }

    internal static EmployeeSkill Create(
        string employeeCode, string operationCode,
        int certificationLevel, DateOnly certifiedAt, DateOnly? expiresAt)
    {
        return new EmployeeSkill
        {
            EmployeeCode = employeeCode,
            OperationCode = operationCode.Trim().ToUpperInvariant(),
            CertificationLevel = certificationLevel,
            CertifiedAt = certifiedAt,
            ExpiresAt = expiresAt,
        };
    }

    internal void Update(int certificationLevel, DateOnly certifiedAt, DateOnly? expiresAt)
    {
        CertificationLevel = certificationLevel;
        CertifiedAt = certifiedAt;
        ExpiresAt = expiresAt;
    }

    public bool IsValidOn(DateOnly date) =>
        ExpiresAt is null || ExpiresAt.Value >= date;
}
