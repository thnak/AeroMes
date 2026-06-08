using MediatR;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public record StartDowntimeCommand(
    int WorkCenterId,
    string MachineCode,
    string ReasonCode,
    string? ReasonName,
    DateTime StartTime,
    string OperatorId
) : IRequest<long>;
