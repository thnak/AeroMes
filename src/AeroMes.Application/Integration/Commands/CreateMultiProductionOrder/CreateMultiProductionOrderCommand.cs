using AeroMes.Application.Common;
using AeroMes.Domain.Integration;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateMultiProductionOrder;

public sealed record CreateMpoLineItem(
    string ProductCode,
    int PlannedQty,
    string UoMCode,
    string? BomVersion);

public sealed record CreateMultiProductionOrderCommand(
    MultiProductionOrderType OrderType,
    string? SourceReference,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    byte Priority,
    string? ProductionUnit,
    string? Notes,
    IReadOnlyList<CreateMpoLineItem> Lines,
    string CreatedBy) : ICommand<ValidationResult<MultiProductionOrderCreatedResult>>;

public sealed record MultiProductionOrderCreatedResult(int MPOId, string OrderNumber, int LineCount);
