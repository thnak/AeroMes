using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerPartNumber;

public class RemoveCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCustomerPartNumberCommand>
{
    public async Task HandleAsync(RemoveCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
        customer.RemovePartNumber(cmd.CustomerPartNumberId);
        await uow.SaveChangesAsync(ct);
    }
}
