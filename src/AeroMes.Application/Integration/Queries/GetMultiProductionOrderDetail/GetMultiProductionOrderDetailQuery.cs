using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetMultiProductionOrderDetail;

public sealed record GetMultiProductionOrderDetailQuery(int MPOId)
    : IQuery<QueryResult<MultiProductionOrderDetailDto>>;

public sealed record MultiProductionOrderDetailDto(
    int MPOId,
    string OrderNumber,
    string OrderType,
    string? SourceReference,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    string Status,
    byte Priority,
    string? ProductionUnit,
    string? Notes,
    DateTime? CreatedAt,
    string? CreatedBy,
    IReadOnlyList<MultiProductionOrderLineDto> Lines);

public sealed record MultiProductionOrderLineDto(
    int LineId,
    int LineNo,
    string ProductCode,
    int PlannedQty,
    string UoMCode,
    string? BomVersion,
    int ActualQtyOK,
    int ActualQtyNG);
