using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master.Events;

public record MoldMountedEvent(int MoldId, string MoldCode, string MachineCode) : IDomainEvent;

public record MoldUnmountedEvent(int MoldId, string MoldCode, string MachineCode) : IDomainEvent;

public record MoldPmDueEvent(
    int MoldId, string MoldCode, long CurrentShots, long ShotsAtLastPm, int PmIntervalShots) : IDomainEvent;

public record MoldNearingEndOfLifeEvent(
    int MoldId, string MoldCode, long CurrentShots, long MaxShots) : IDomainEvent;
