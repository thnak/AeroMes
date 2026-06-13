using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.BatchCreateProductionOrders;

public sealed record BatchOrderItem(
    string ProductCode,
    int TargetQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ProductionDeadline,
    byte Priority,
    string? AssignedTo);

public sealed record BatchCreateProductionOrdersCommand(
    IReadOnlyList<BatchOrderItem> Items,
    string CreatedBy) : ICommand<ValidationResult<BatchCreateResult>>;

public sealed record BatchCreateResult(int OrdersCreated, IReadOnlyList<string> POCodes);
