using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetReplenishmentAlerts;

public record GetReplenishmentAlertsQuery(ReplenishmentAlertStatus? Status)
    : IQuery<IReadOnlyList<ReplenishmentAlertDto>>;

public record ReplenishmentAlertDto(
    long AlertId,
    int PolicyId,
    string ProductCode,
    int LocationId,
    decimal CurrentQty,
    DateTime TriggeredAt,
    ReplenishmentAlertStatus Status,
    string? AcknowledgedBy,
    DateTime? AcknowledgedAt,
    int? LinkedPoId);
