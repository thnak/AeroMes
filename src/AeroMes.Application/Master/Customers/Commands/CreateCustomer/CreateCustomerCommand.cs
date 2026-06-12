using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Code,
    string Name,
    CustomerType CustomerType,
    string? TaxId,
    string? Country,
    string? Address,
    string? ShippingAddress,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    int CreditTermsDays,
    string? Currency,
    string? Notes,
    string? CreatedBy) : ICommand<string>;
