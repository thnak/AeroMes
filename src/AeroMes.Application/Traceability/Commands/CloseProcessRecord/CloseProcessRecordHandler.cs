using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CloseProcessRecord;

public sealed class CloseProcessRecordHandler(
    IProcessRecordRepository repository,
    ILotTraceabilityRepository traceabilityRepository,
    IValidator<CloseProcessRecordCommand> validator)
    : ICommandHandler<CloseProcessRecordCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CloseProcessRecordCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var record = await repository.GetByIdAsync(command.ProcessRecordID, ct);
        if (record is null) return ValidationResult<Unit>.NotFound(
            $"Process record {command.ProcessRecordID} not found.");

        try
        {
            record.Close(command.Outcome, command.DeviationRef);

            // Crystallize LotLineage edge when an output lot is produced
            if (!string.IsNullOrWhiteSpace(command.OutputLotNumber) && command.Outcome == StepOutcome.Pass)
            {
                var lineage = LotLineage.Record(
                    record.LotNumber,
                    command.OutputLotNumber,
                    LineageType.Transform,
                    record.WorkOrderID,
                    record.RoutingStepID,
                    quantityConsumed: null,
                    uom: null);
                await traceabilityRepository.AddLineageAsync(lineage, ct);
            }

            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
