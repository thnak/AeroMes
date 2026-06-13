using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomer;

public class UpdateCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateCustomerCommand> validator) : ICommandHandler<UpdateCustomerCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var customer = await repo.GetByIdAsync(cmd.Code, ct);
            if (customer is null)
                return ValidationResult<Unit>.NotFound($"Customer '{cmd.Code}' not found.");

            customer.UpdateDetails(
                cmd.Name, cmd.CustomerType,
                cmd.TaxId, cmd.Country, cmd.Address, cmd.ShippingAddress,
                cmd.ContactName, cmd.ContactPhone, cmd.ContactEmail,
                cmd.CreditTermsDays, cmd.Currency, cmd.Notes,
                cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
