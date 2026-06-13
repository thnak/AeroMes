using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RecordMoldShots;

public class RecordMoldShotsHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<RecordMoldShotsCommand> validator) : ICommandHandler<RecordMoldShotsCommand, ValidationResult<RecordMoldShotsResult>>
{
    public async Task<ValidationResult<RecordMoldShotsResult>> HandleAsync(RecordMoldShotsCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<RecordMoldShotsResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null)
                return ValidationResult<RecordMoldShotsResult>.NotFound($"{nameof(Mold)} '{cmd.MoldCode}' not found.");

            mold.AccumulateShots(cmd.Shots, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<RecordMoldShotsResult>.Ok(
                new RecordMoldShotsResult(
                    mold.CurrentShots, mold.MaxShots, mold.IsPmDue, mold.IsNearingEndOfLife));
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<RecordMoldShotsResult>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<RecordMoldShotsResult>.Failure(ex.Message);
        }
    }
}
