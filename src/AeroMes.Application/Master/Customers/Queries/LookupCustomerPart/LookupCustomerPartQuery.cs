using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;

public record LookupCustomerPartQuery(string CustomerCode, string CustomerPartNo)
    : IQuery<CustomerPartLookupDto?>;

public record CustomerPartLookupDto(
    string CustomerCode,
    string CustomerPartNo,
    string ProductCode,
    string? ProductName,
    string? Revision);
