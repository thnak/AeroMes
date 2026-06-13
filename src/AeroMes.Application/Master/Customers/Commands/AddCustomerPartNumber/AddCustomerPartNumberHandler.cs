using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Customers.Commands.AddCustomerPartNumber;

public class AddCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow,
    IValidator<AddCustomerPartNumberCommand> validator) : ICommandHandler<AddCustomerPartNumberCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct);
            if (customer is null) return ValidationResult<int>.NotFound($"Entity '{cmd.CustomerCode}' was not found.");
            var partNumber = customer.AddPartNumber(
                cmd.CustomerPartNo, cmd.ProductCode,
                cmd.Description, cmd.DrawingReference, cmd.Revision);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(partNumber.CustomerPartNumberId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
