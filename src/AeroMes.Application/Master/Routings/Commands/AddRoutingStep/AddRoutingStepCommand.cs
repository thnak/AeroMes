using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.AddRoutingStep;

public record AddRoutingStepCommand(
    int RoutingId,
    int StepNumber,
    string OperationCode,
    int DefaultWorkCenterId,
    double StandardCycleTime,
    bool IsQcRequired) : ICommand<ValidationResult<int>>;
