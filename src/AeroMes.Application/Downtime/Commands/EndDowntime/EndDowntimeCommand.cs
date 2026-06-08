using MediatR;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public record EndDowntimeCommand(long DowntimeLogId, DateTime EndTime, string OperatorId)
    : IRequest<EndDowntimeResult>;

public record EndDowntimeResult(long DowntimeLogId, int DurationMinutes);
