using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public record StartDowntimeCommand(
    string MachineCode,
    string ReasonCode,
    string? ReasonName,
    DateTime StartTime,
    string OperatorId,
    string? Notes = null) : ICommand<ValidationResult<long>>;
