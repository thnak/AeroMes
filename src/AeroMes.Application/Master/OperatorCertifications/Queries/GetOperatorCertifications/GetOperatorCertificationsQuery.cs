using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Queries.GetOperatorCertifications;

public record GetOperatorCertificationsQuery(string EmployeeCode)
    : IQuery<IReadOnlyList<OperatorCertificationDto>>;

public record OperatorCertificationDto(
    int CertId,
    string EmployeeCode,
    string CertificationCode,
    DateOnly IssuedDate,
    DateOnly? ExpiryDate,
    string? IssuedBy,
    bool IsActive);
