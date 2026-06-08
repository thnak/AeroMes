using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Equipment.Events;

namespace AeroMes.Domain.Equipment;

public class DowntimeLog : Entity
{
    public long DowntimeLogID { get; private set; }
    public int WorkCenterID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string ReasonCode { get; private set; } = string.Empty;
    public string? ReasonName { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string? OperatorID { get; private set; }
    public string? Notes { get; private set; }

    public int? DurationMinutes =>
        EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalMinutes : null;

    private DowntimeLog() { } // EF constructor

    public static DowntimeLog Create(
        int workCenterId,
        string machineCode,
        string reasonCode,
        string? reasonName,
        DateTime startTime,
        string operatorId)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new DomainException("MachineCode is required.");
        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new DomainException("ReasonCode is required.");

        var log = new DowntimeLog
        {
            WorkCenterID = workCenterId,
            MachineCode = machineCode,
            ReasonCode = reasonCode,
            ReasonName = reasonName,
            StartTime = startTime,
            OperatorID = operatorId,
        };
        log.RaiseDomainEvent(new DowntimeStartedEvent(
            log.DowntimeLogID, workCenterId, machineCode, reasonCode));
        return log;
    }

    public void End(DateTime endTime)
    {
        if (EndTime.HasValue)
            throw new DomainException($"DowntimeLog {DowntimeLogID} is already closed.");
        if (endTime < StartTime)
            throw new DomainException("End time cannot be before start time.");
        EndTime = endTime;
        RaiseDomainEvent(new DowntimeEndedEvent(
            DowntimeLogID, WorkCenterID, DurationMinutes!.Value));
    }
}
