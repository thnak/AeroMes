using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

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
            var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
                ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
            var partNumber = customer.AddPartNumber(
                cmd.CustomerPartNo, cmd.ProductCode,
                cmd.Description, cmd.DrawingReference, cmd.Revision);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(partNumber.CustomerPartNumberId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
