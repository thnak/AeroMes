using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.AddRoutingStep;

public record AddRoutingStepCommand(
    int RoutingId,
    int StepNumber,
    string OperationCode,
    int DefaultWorkCenterId,
    double StandardCycleTime,
    bool IsQcRequired) : IRequest<int>;
