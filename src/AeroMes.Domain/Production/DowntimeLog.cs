using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Events;

namespace AeroMes.Domain.Production;

public class DowntimeLog : Entity
{
    public long DowntimeLogID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string ReasonCode { get; private set; } = string.Empty;
    public string? ReasonName { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string? OperatorID { get; private set; }
    public string? Notes { get; private set; }

    public int? DurationMinutes =>
        EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalMinutes : null;

    private DowntimeLog() { }

    public static DowntimeLog Create(
        string machineCode,
        string reasonCode,
        string? reasonName,
        DateTime startTime,
        string operatorId,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new DomainException("Machine code is required.");
        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new DomainException("Reason code is required.");

        var log = new DowntimeLog
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            ReasonCode = reasonCode.Trim().ToUpperInvariant(),
            ReasonName = reasonName,
            StartTime = startTime,
            OperatorID = operatorId,
            Notes = notes,
        };
        log.RaiseDomainEvent(new DowntimeStartedEvent(log.DowntimeLogID, machineCode, reasonCode));
        return log;
    }

    public void End(DateTime endTime, string? notes = null)
    {
        if (EndTime.HasValue)
            throw new DomainException($"DowntimeLog {DowntimeLogID} is already closed.");
        if (endTime < StartTime)
            throw new DomainException("End time cannot be before start time.");
        EndTime = endTime;
        if (notes is not null) Notes = notes;
        RaiseDomainEvent(new DowntimeEndedEvent(DowntimeLogID, MachineCode, DurationMinutes!.Value));
    }
}
