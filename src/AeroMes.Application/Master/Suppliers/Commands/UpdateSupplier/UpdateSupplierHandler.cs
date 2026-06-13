using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateSupplierCommand> validator) : ICommandHandler<UpdateSupplierCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateSupplierCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var supplier = await repo.GetByIdAsync(cmd.Code, ct);
            if (supplier is null)
                return ValidationResult<Unit>.NotFound($"{nameof(cmd.Code)}: {cmd.Code}");
            supplier.UpdateDetails(
                cmd.Name, cmd.Country, cmd.City, cmd.Address,
                cmd.Phone, cmd.Email, cmd.ContactName, cmd.TaxCode,
                cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
