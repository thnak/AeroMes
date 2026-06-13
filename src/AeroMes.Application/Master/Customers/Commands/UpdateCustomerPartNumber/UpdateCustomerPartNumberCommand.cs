using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomerPartNumber;

public record UpdateCustomerPartNumberCommand(
    string CustomerCode,
    int CustomerPartNumberId,
    string? Description,
    string? DrawingReference,
    string? Revision,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
