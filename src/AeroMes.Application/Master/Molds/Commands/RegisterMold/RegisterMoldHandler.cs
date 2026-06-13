using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.RegisterMold;
public class RegisterMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<RegisterMoldCommand> validator) : ICommandHandler<RegisterMoldCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(RegisterMoldCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());
        try
        {
            var mold = Mold.Create(
                cmd.Code, cmd.Name, cmd.MoldType,
                cmd.Material, cmd.Cavities, cmd.MaxShots, cmd.PmIntervalShots,
                cmd.Manufacturer, cmd.PurchaseDate, cmd.PurchaseCost, cmd.WeightKg,
                cmd.StorageLocation, cmd.Notes, cmd.CreatedBy);
            await repo.AddAsync(mold, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(mold.MoldCode);
        }        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
