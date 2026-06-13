using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.CreateProcessStep;

public record CreateProcessStepCommand(
    string Code,
    string Name,
    string? Description,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
