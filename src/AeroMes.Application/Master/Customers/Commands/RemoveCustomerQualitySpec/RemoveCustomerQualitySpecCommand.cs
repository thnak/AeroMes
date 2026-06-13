using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerQualitySpec;

public record RemoveCustomerQualitySpecCommand(
    string CustomerCode,
    int CustomerQualitySpecId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
