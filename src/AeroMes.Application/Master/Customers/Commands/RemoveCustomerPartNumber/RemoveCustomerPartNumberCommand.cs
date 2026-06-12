using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerPartNumber;

public record RemoveCustomerPartNumberCommand(
    string CustomerCode,
    int CustomerPartNumberId,
    string? DeletedBy) : ICommand;
