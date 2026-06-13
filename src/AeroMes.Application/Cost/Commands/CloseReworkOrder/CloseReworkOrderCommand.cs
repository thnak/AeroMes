using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.CloseReworkOrder;

public record CloseReworkOrderCommand(
    int ReworkID,
    decimal ActMaterialCost,
    decimal ActLaborCost,
    decimal ActMachineCost,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
