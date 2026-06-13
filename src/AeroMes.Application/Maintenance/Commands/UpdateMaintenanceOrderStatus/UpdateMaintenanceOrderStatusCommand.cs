using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.UpdateMaintenanceOrderStatus;

public record UpdateMaintenanceOrderStatusCommand(
    int MaintOrderID,
    string Action,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
