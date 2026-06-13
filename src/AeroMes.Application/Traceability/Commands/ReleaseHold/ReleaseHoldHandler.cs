using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ReleaseHold;

public sealed class ReleaseHoldHandler(
    ILotHoldRepository repository,
    IESignatureService eSignatureService,
    IValidator<ReleaseHoldCommand> validator)
    : ICommandHandler<ReleaseHoldCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ReleaseHoldCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var hold = await repository.GetByIdAsync(command.HoldID, ct);
        if (hold is null) return ValidationResult<Unit>.NotFound($"Hold {command.HoldID} not found.");

        try
        {
            var signatureRef = await eSignatureService.ValidateAndRecordAsync(
                command.ReleasedBy,
                command.ESignatureToken,
                $"I approve release of hold {command.HoldID} on lot {hold.LotNumber}",
                ct);

            hold.Release(command.DispositionCode, command.ReleasedBy, signatureRef, command.DispositionNotes);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
