using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequestById;

public record GetFinishedProductIntakeRequestByIdQuery(int IntakeRequestId)
    : IQuery<FinishedProductIntakeRequestDetailDto?>;

public record FinishedProductIntakeRequestDetailDto(
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
    IReadOnlyList<IntakeRequestLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record IntakeRequestLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int WarehouseId,
    bool IsDefective,
    string? DefectReason,
    decimal? ActualReceivedQuantity,
    string? Notes);
