using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(string Code, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
