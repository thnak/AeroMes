using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality.Events;

public record InspectionOrderFailedEvent(int InspectionOrderId, long WorkOrderId, string ProductCode) : IDomainEvent;
