using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.GenerateCycleCountLines;

public record GenerateCycleCountLinesCommand(
    int PlanId,
    int[]? BinIds,
    string? GeneratedBy
) : ICommand<ValidationResult<Unit>>;
