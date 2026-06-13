using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.SetStepStatus;

public record SetStepStatusCommand(int StepID, bool Activate, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
