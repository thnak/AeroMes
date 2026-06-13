using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CloseRecall;

public sealed class CloseRecallHandler(
    IRecallRepository recallRepository,
    IESignatureService eSignatureService,
    IValidator<CloseRecallCommand> validator)
    : ICommandHandler<CloseRecallCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CloseRecallCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var recall = await recallRepository.GetByIdAsync(command.RecallID, ct);
        if (recall is null) return ValidationResult<Unit>.NotFound($"Recall {command.RecallID} not found.");

        var hasUnresolved = await recallRepository.HasUnresolvedHoldsAsync(command.RecallID, ct);
        if (hasUnresolved)
            return ValidationResult<Unit>.Failure(
                "Cannot close recall: some holds are still in Active state. Dispose all holds first.");

        try
        {
            await eSignatureService.ValidateAndRecordAsync(
                command.ClosedBy,
                command.ESignatureToken,
                $"I approve closure of recall {recall.RecallCode}",
                ct);

            recall.Close(command.ClosedBy);

            await recallRepository.AddAuditEntryAsync(
                RecallAuditEntry.Log(recall.RecallID, "Closed", command.ClosedBy,
                    command.ClosureNotes),
                ct);

            await recallRepository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
