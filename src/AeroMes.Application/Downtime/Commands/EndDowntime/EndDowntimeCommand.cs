using MediatR;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public record EndDowntimeCommand(long DowntimeLogId, DateTime EndTime, string? Notes = null) : IRequest<EndDowntimeResult>;

public record EndDowntimeResult(long DowntimeLogId, int DurationMinutes, string Status);
