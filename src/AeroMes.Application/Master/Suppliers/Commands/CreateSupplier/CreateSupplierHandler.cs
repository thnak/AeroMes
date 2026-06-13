using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Suppliers.Commands.CreateSupplier;
public class CreateSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow,
    IValidator<CreateSupplierCommand> validator) : ICommandHandler<CreateSupplierCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateSupplierCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());
        try
        {
            var supplier = Supplier.Create(
                cmd.Code, cmd.Name,
                cmd.Country, cmd.City, cmd.Address,
                cmd.Phone, cmd.Email, cmd.ContactName, cmd.TaxCode,
                cmd.CreatedBy);
            await repo.AddAsync(supplier, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(supplier.SupplierCode);
        }        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
