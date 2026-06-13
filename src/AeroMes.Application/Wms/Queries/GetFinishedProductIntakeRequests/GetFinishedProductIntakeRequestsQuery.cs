using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequests;

public record GetFinishedProductIntakeRequestsQuery(
    IntakeRequestPurpose? IntakePurpose,
    IntakeRequestStatus? Status,
    int? ProductionOrderId
) : IQuery<IReadOnlyList<FinishedProductIntakeRequestSummaryDto>>;

public record FinishedProductIntakeRequestSummaryDto(
    int IntakeRequestId,
    string RequestNumber,
    IntakeRequestPurpose IntakePurpose,
    IntakeWarehouseType WarehouseType,
    IntakeRequestStatus Status,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    DateTime? SentAt,
    string? Notes,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
