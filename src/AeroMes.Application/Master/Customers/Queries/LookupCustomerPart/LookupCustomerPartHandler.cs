using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;

public class LookupCustomerPartHandler(ICustomerRepository repo)
    : IQueryHandler<LookupCustomerPartQuery, CustomerPartLookupDto?>
{
    public async Task<CustomerPartLookupDto?> HandleAsync(LookupCustomerPartQuery query, CancellationToken ct)
    {
        var partNumber = await repo.GetPartNumberAsync(query.CustomerCode, query.CustomerPartNo, ct);
        if (partNumber is null) return null;

        return new CustomerPartLookupDto(
            partNumber.CustomerCode,
            partNumber.CustomerPartNo,
            partNumber.ProductCode,
            partNumber.Product?.ProductName,
            partNumber.Revision);
    }
}
