using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RecordProcessParameter;

public sealed class RecordProcessParameterHandler(
    IProcessRecordRepository repository,
    IValidator<RecordProcessParameterCommand> validator)
    : ICommandHandler<RecordProcessParameterCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        RecordProcessParameterCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var record = await repository.GetByIdAsync(command.ProcessRecordID, ct);
        if (record is null) return ValidationResult<Unit>.NotFound(
            $"Process record {command.ProcessRecordID} not found.");

        try
        {
            record.AddParameter(
                command.ParameterName,
                command.ActualValue,
                command.NominalValue,
                command.UoM,
                command.LSL,
                command.USL,
                command.DataSource);

            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
