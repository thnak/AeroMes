using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot.Events;

public record AdapterReconnectedEvent(int AdapterId, string MachineCode) : IDomainEvent;
