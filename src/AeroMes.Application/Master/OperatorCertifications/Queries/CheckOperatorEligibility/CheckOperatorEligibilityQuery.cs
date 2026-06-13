using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Queries.CheckOperatorEligibility;

public record CheckOperatorEligibilityQuery(
    string EmployeeCode,
    string MachineCode) : IQuery<OperatorEligibilityResult>;

public record OperatorEligibilityResult(
    bool IsEligible,
    string Reason,
    string? CertificationCode,
    DateOnly? CertExpiry);
