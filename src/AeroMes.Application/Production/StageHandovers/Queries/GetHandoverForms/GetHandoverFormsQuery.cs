using AeroMes.Domain.Production;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Queries.GetHandoverForms;

public sealed record GetHandoverFormsQuery(int? WorkOrderId, DateTime? From, DateTime? To)
    : IQuery<IReadOnlyList<HandoverFormSummaryDto>>;

public sealed record HandoverFormSummaryDto(
    int FormId,
    string FormNumber,
    string FormType,
    string Status,
    int FromWorkOrderId,
    int FromRoutingStepId,
    int ToWorkOrderId,
    int ToRoutingStepId,
    DateTime HandoverDate,
    int LineCount,
    string? CreatedBy,
    DateTime CreatedAt);
