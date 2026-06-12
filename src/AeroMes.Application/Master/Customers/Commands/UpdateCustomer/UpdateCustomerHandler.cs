using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomer;

public class UpdateCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateCustomerCommand>
{
    public async Task HandleAsync(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        customer.UpdateDetails(
            cmd.Name, cmd.CustomerType,
            cmd.TaxId, cmd.Country, cmd.Address, cmd.ShippingAddress,
            cmd.ContactName, cmd.ContactPhone, cmd.ContactEmail,
            cmd.CreditTermsDays, cmd.Currency, cmd.Notes,
            cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
