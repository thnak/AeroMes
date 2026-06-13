using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerPartNumber;

public record RemoveCustomerPartNumberCommand(
    string CustomerCode,
    int CustomerPartNumberId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
