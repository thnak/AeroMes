using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.DeleteCustomer;

public class DeleteCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteCustomerCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdAsync(cmd.Code, ct);
        if (customer is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.Code}' was not found.");
        customer.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
