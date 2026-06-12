using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerQualitySpec;

public record RemoveCustomerQualitySpecCommand(
    string CustomerCode,
    int CustomerQualitySpecId,
    string? DeletedBy) : ICommand;
