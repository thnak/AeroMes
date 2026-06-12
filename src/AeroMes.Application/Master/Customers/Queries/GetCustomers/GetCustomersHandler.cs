using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.GetCustomers;

public class GetCustomersHandler(ICustomerRepository repo)
    : IQueryHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>
{
    public async Task<IReadOnlyList<CustomerDto>> HandleAsync(GetCustomersQuery query, CancellationToken ct)
    {
        var customers = await repo.GetAllAsync(query.ActiveOnly, ct);
        return customers
            .Select(c => new CustomerDto(
                c.CustomerCode, c.CustomerName,
                c.CustomerType.ToString(),
                c.Country, c.ContactName, c.ContactPhone, c.ContactEmail,
                c.Currency, c.IsActive, c.PartNumbers.Count))
            .ToList();
    }
}
