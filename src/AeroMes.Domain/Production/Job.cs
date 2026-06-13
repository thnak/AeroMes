using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production.Events;

namespace AeroMes.Domain.Production;

public enum JobStatus { Active, Suspended, Finished }

public class Job : Entity
{
    public long JobID { get; private set; }
    public int WOID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string ShiftCode { get; private set; } = string.Empty;
    public string OperatorID { get; private set; } = string.Empty;
    public string? MoldCode { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Active;

    // EF navigations
    public WorkOrder? WorkOrder { get; private set; }
    public Machine? Machine { get; private set; }
    public Employee? Operator { get; private set; }

    private Job() { }

    public static Job Create(
        int woId,
        string machineCode,
        string shiftCode,
        string operatorId,
        DateTime? startTime = null)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new DomainException("Machine code is required.");
        if (string.IsNullOrWhiteSpace(shiftCode))
            throw new DomainException("Shift code is required.");
        if (string.IsNullOrWhiteSpace(operatorId))
            throw new DomainException("Operator ID is required.");

        var job = new Job
        {
            WOID = woId,
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            ShiftCode = shiftCode.Trim().ToUpperInvariant(),
            OperatorID = operatorId.Trim(),
            StartTime = startTime ?? DateTime.UtcNow,
            Status = JobStatus.Active,
        };
        job.RaiseDomainEvent(new JobStartedEvent(job.JobID, woId, machineCode, operatorId));
        return job;
    }

    public void Suspend()
    {
        EnsureStatus(JobStatus.Active, nameof(Suspend));
        Status = JobStatus.Suspended;
    }

    public void Resume()
    {
        EnsureStatus(JobStatus.Suspended, nameof(Resume));
        Status = JobStatus.Active;
    }

    public void Finish(DateTime? endTime = null)
    {
        if (Status == JobStatus.Finished)
            throw new DomainException($"Job {JobID} is already finished.");
        Status = JobStatus.Finished;
        EndTime = endTime ?? DateTime.UtcNow;
        RaiseDomainEvent(new JobFinishedEvent(JobID, WOID));
    }

    public void AssignMold(string moldCode)
    {
        if (Status == JobStatus.Finished)
            throw new DomainException($"Cannot assign mold to a finished job {JobID}.");
        MoldCode = moldCode.Trim().ToUpperInvariant();
    }

    private void EnsureStatus(JobStatus required, string operation)
    {
        if (Status != required)
            throw new DomainException(
                $"Job {JobID} must be {required} to {operation}. Current: {Status}.");
    }
}
