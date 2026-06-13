using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.InitiateRecall;

public sealed class InitiateRecallHandler(
    IRecallRepository repository,
    IValidator<InitiateRecallCommand> validator)
    : ICommandHandler<InitiateRecallCommand, ValidationResult<Guid>>
{
    public async Task<ValidationResult<Guid>> HandleAsync(
        InitiateRecallCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Guid>.Invalid(vr.ToErrorDictionary());

        try
        {
            var count = await repository.CountAsync(ct);
            var recallCode = $"RECALL-{DateTime.UtcNow:yyyy}-{count + 1:D4}";

            var recall = Recall.Initiate(
                recallCode, command.Title, command.RecallType,
                command.AnchorLotNumber, command.AnchorDirection,
                command.InitiatedBy, command.Description, command.RegulatoryRef);

            await repository.AddAsync(recall, ct);
            await repository.AddAuditEntryAsync(
                RecallAuditEntry.Log(recall.RecallID, "Initiated",
                    command.InitiatedBy,
                    $"Recall {recallCode} initiated on anchor lot {command.AnchorLotNumber}"),
                ct);
            await repository.SaveChangesAsync(ct);

            return ValidationResult<Guid>.Ok(recall.RecallID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Guid>.Failure(ex.Message);
        }
    }
}
