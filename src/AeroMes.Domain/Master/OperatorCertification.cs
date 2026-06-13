using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class OperatorCertification : Entity
{
    public int CertId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string CertificationCode { get; private set; } = string.Empty;
    public DateOnly IssuedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? IssuedBy { get; private set; }
    public bool IsActive { get; private set; } = true;

    private OperatorCertification() { }

    public static OperatorCertification Create(
        string employeeCode,
        string certificationCode,
        DateOnly issuedDate,
        DateOnly? expiryDate,
        string? issuedBy)
    {
        return new OperatorCertification
        {
            EmployeeCode = employeeCode.Trim().ToUpperInvariant(),
            CertificationCode = certificationCode.Trim().ToUpperInvariant(),
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            IssuedBy = issuedBy,
            IsActive = true,
        };
    }

    public void Revoke() => IsActive = false;
}
