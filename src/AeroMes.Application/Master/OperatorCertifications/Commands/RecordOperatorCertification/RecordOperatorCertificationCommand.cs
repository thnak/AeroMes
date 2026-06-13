using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Commands.RecordOperatorCertification;

public record RecordOperatorCertificationCommand(
    string EmployeeCode,
    string CertificationCode,
    DateOnly IssuedDate,
    DateOnly? ExpiryDate,
    string? IssuedBy) : ICommand<ValidationResult<int>>;
