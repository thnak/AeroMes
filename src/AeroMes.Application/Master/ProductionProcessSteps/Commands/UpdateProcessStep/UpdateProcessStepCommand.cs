using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.UpdateProcessStep;

public record UpdateProcessStepCommand(
    int StepID,
    string Name,
    string? Description,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
