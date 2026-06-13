using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.CreateProductionProcess;

public record StageInput(
    int SortOrder,
    string? ProcessStepCode,
    StageCapacityType CapacityType,
    string CapacityIdsJson,
    decimal PlannedTimeSeconds,
    PlannedTimeSource PlannedTimeSource,
    int TimeOffsetDays,
    bool IsPrimaryStage);

public record CreateProductionProcessCommand(
    string Code,
    string Name,
    ProductionProcessType ProcessType,
    DateOnly EffectiveDate,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    bool IsForPlanningOnly,
    IReadOnlyList<StageInput> Stages,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
