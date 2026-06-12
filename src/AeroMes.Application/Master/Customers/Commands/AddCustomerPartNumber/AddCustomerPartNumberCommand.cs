using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.AddCustomerPartNumber;

public record AddCustomerPartNumberCommand(
    string CustomerCode,
    string CustomerPartNo,
    string ProductCode,
    string? Description,
    string? DrawingReference,
    string? Revision,
    string? CreatedBy) : ICommand<int>;
