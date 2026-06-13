using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.ConfirmSalesOrder;

public record ConfirmSalesOrderCommand(
    int SOID,
    string? FacilityCode,
    string ConfirmedBy) : ICommand<ValidationResult<Unit>>;
