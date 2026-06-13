using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality.Events;

public record AQLRejectedEvent(
    int WOID,
    int DefectsFound,
    int RejectionNumber,
    string AQLLevel) : IDomainEvent;
