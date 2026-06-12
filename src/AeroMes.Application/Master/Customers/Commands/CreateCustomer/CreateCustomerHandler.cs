using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateCustomerCommand, string>
{
    public async Task<string> HandleAsync(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = Customer.Create(
            cmd.Code, cmd.Name, cmd.CustomerType,
            cmd.TaxId, cmd.Country, cmd.Address, cmd.ShippingAddress,
            cmd.ContactName, cmd.ContactPhone, cmd.ContactEmail,
            cmd.CreditTermsDays, cmd.Currency, cmd.Notes,
            cmd.CreatedBy);
        await repo.AddAsync(customer, ct);
        await uow.SaveChangesAsync(ct);
        return customer.CustomerCode;
    }
}
