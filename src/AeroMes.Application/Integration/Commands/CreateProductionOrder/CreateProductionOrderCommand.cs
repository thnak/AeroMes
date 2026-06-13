using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateProductionOrder;

public record CreateProductionOrderCommand(
    string ProductCode,
    int TargetQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? Deadline,
    byte Priority,
    string? AssignedTo,
    int? SoId,
    int? RoutingId,
    bool AutoExpandBom,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
