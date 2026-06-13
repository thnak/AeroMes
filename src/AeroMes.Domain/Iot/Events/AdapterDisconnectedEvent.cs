using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot.Events;

public record AdapterDisconnectedEvent(int AdapterId, string MachineCode, string Reason) : IDomainEvent;
