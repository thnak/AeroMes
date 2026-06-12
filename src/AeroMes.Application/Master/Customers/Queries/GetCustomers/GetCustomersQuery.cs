using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.GetCustomers;

public record GetCustomersQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<CustomerDto>>;

public record CustomerDto(
    string CustomerCode,
    string CustomerName,
    string CustomerType,
    string? Country,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    string Currency,
    bool IsActive,
    int PartNumberCount);
