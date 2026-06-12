using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(string Code, string? DeletedBy) : ICommand;
