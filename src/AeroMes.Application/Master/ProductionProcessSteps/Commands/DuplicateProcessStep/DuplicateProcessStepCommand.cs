using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.DuplicateProcessStep;

public record DuplicateProcessStepCommand(int StepID, string NewCode, string? CreatedBy)
    : ICommand<ValidationResult<int>>;
