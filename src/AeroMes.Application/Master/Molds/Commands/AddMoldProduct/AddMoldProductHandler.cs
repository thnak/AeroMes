using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.AddMoldProduct;

public class AddMoldProductHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<AddMoldProductCommand> validator) : ICommandHandler<AddMoldProductCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddMoldProductCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null) return ValidationResult<int>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

            var mapping = mold.AddProductMapping(
                cmd.ProductCode, cmd.IsDefault, cmd.CycleTimeSeconds, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(mapping.MappingId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
