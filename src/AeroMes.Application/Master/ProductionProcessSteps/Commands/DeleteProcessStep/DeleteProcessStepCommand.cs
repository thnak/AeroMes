using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.DeleteProcessStep;

public record DeleteProcessStepCommand(int StepID) : ICommand<ValidationResult<Unit>>;
