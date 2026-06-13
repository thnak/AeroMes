using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomerPartNumber;

public class UpdateCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateCustomerPartNumberCommand> validator) : ICommandHandler<UpdateCustomerPartNumberCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct);
            if (customer is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.CustomerCode}' was not found.");
            customer.UpdatePartNumber(
                cmd.CustomerPartNumberId,
                cmd.Description, cmd.DrawingReference, cmd.Revision);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
