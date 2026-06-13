using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Queries.GetHandoverFormDetail;

public sealed record GetHandoverFormDetailQuery(int FormId) : IQuery<HandoverFormDetailDto?>;

public sealed record HandoverFormDetailDto(
    int FormId,
    string FormNumber,
    string FormType,
    string Status,
    int FromWorkOrderId,
    int FromRoutingStepId,
    int ToWorkOrderId,
    int ToRoutingStepId,
    DateTime HandoverDate,
    string? Notes,
    string? CreatedBy,
    DateTime CreatedAt,
    IReadOnlyList<HandoverLineDto> Lines);

public sealed record HandoverLineDto(
    int LineId,
    string ProductCode,
    decimal Qty,
    string Unit,
    string? Notes);
