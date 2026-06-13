using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.OpenProcessRecord;

public sealed class OpenProcessRecordHandler(
    IProcessRecordRepository repository,
    IValidator<OpenProcessRecordCommand> validator)
    : ICommandHandler<OpenProcessRecordCommand, ValidationResult<Guid>>
{
    public async Task<ValidationResult<Guid>> HandleAsync(
        OpenProcessRecordCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Guid>.Invalid(vr.ToErrorDictionary());

        try
        {
            var record = ProcessRecord.Open(
                command.LotNumber,
                command.ProductCode,
                command.WorkOrderID,
                command.JobID,
                command.RoutingStepID,
                command.StepSequence,
                command.StepName,
                command.OperatorCode,
                command.MachineCode,
                command.BOMRevision,
                command.RoutingRevision,
                command.ControlPlanRev,
                command.CertificationRef,
                command.CalibrationRef);

            await repository.AddAsync(record, ct);
            await repository.SaveChangesAsync(ct);

            return ValidationResult<Guid>.Ok(record.ProcessRecordID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Guid>.Failure(ex.Message);
        }
    }
}
