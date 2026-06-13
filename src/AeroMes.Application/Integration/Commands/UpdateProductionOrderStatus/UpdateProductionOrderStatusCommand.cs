using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.UpdateProductionOrderStatus;

public record UpdateProductionOrderStatusCommand(
    int Id,
    string Action) : ICommand<ValidationResult<Unit>>;
// Action: "Start" | "Pause" | "Resume" | "Complete" | "Cancel"
