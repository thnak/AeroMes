using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.DeleteCustomer;

public class DeleteCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteCustomerCommand>
{
    public async Task HandleAsync(DeleteCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        customer.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
