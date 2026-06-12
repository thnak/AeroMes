using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.AddCustomerPartNumber;

public class AddCustomerPartNumberHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddCustomerPartNumberCommand, int>
{
    public async Task<int> HandleAsync(AddCustomerPartNumberCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
        var partNumber = customer.AddPartNumber(
            cmd.CustomerPartNo, cmd.ProductCode,
            cmd.Description, cmd.DrawingReference, cmd.Revision);
        await uow.SaveChangesAsync(ct);
        return partNumber.CustomerPartNumberId;
    }
}
