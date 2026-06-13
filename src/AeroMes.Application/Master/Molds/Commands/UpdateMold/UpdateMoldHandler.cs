using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.UpdateMold;

public class UpdateMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateMoldCommand> validator) : ICommandHandler<UpdateMoldCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMoldCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var mold = await repo.GetByCodeAsync(cmd.Code, ct);
            if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.Code}' was not found.");

            mold.UpdateDetails(
                cmd.Name, cmd.MoldType,
                cmd.Material, cmd.Cavities, cmd.MaxShots, cmd.PmIntervalShots,
                cmd.Manufacturer, cmd.PurchaseDate, cmd.PurchaseCost, cmd.WeightKg,
                cmd.StorageLocation, cmd.Notes, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
