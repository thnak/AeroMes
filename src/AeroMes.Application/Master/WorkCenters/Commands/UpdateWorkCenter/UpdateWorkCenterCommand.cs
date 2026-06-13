using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;

public record UpdateWorkCenterCommand(
    int Id,
    string Name,
    string? Description,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
