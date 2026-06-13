using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerPartNumber;

public class RemoveCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCustomerPartNumberCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct);
        if (customer is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.CustomerCode}' was not found.");
        customer.RemovePartNumber(cmd.CustomerPartNumberId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
