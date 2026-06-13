using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality.Events;

public record AQLAcceptedEvent(int WOID, string AQLLevel) : IDomainEvent;
