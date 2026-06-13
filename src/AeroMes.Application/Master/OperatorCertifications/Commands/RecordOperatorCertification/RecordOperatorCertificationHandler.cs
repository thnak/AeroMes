using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Commands.RecordOperatorCertification;

public class RecordOperatorCertificationHandler(
    IOperatorCertificationRepository repo,
    IEmployeeRepository employeeRepo,
    IUnitOfWork uow,
    IValidator<RecordOperatorCertificationCommand> validator) : ICommandHandler<RecordOperatorCertificationCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(RecordOperatorCertificationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (!await employeeRepo.CodeExistsAsync(cmd.EmployeeCode, ct))
                throw new EntityNotFoundException("Employee", cmd.EmployeeCode);

            var cert = OperatorCertification.Create(
                cmd.EmployeeCode,
                cmd.CertificationCode,
                cmd.IssuedDate,
                cmd.ExpiryDate,
                cmd.IssuedBy);

            await repo.AddAsync(cert, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(cert.CertId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
