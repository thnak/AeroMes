using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public record EndDowntimeCommand(long DowntimeLogId, DateTime EndTime, string? Notes = null) : ICommand<ValidationResult<EndDowntimeResult>>;

public record EndDowntimeResult(long DowntimeLogId, int DurationMinutes, string Status);
