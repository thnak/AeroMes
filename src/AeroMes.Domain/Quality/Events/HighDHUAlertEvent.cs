using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality.Events;

public record HighDHUAlertEvent(
    int WOID,
    int WorkCenterID,
    string StyleCode,
    decimal DHU,
    decimal DHU_Target) : IDomainEvent;
