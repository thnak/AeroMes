using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot.Events;

public record MachineSignalIngestedEvent(
    string MachineCode, string TagKey, decimal Value, string? Unit,
    DateTimeOffset Timestamp, bool IsBadQuality) : IDomainEvent;
