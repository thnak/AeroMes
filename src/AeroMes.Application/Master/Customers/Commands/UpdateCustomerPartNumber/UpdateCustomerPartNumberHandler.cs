using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomerPartNumber;

public class UpdateCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateCustomerPartNumberCommand>
{
    public async Task HandleAsync(UpdateCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
        customer.UpdatePartNumber(
            cmd.CustomerPartNumberId,
            cmd.Description, cmd.DrawingReference, cmd.Revision);
        await uow.SaveChangesAsync(ct);
    }
}
