using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.UpdateProductionProcess;

public record UpdateProductionProcessCommand(
    int ProcessID,
    string Name,
    DateOnly EffectiveDate,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    bool IsForPlanningOnly,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
