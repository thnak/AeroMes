using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow,
    IValidator<CreateCustomerCommand> validator) : ICommandHandler<CreateCustomerCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var customer = Customer.Create(
                cmd.Code, cmd.Name, cmd.CustomerType,
                cmd.TaxId, cmd.Country, cmd.Address, cmd.ShippingAddress,
                cmd.ContactName, cmd.ContactPhone, cmd.ContactEmail,
                cmd.CreditTermsDays, cmd.Currency, cmd.Notes,
                cmd.CreatedBy);
            await repo.AddAsync(customer, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(customer.CustomerCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
