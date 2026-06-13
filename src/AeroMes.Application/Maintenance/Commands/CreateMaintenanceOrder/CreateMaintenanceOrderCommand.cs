using AeroMes.Application.Common;
using AeroMes.Domain.Maintenance;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.CreateMaintenanceOrder;

public record CreateMaintenanceOrderCommand(
    string MaintOrderCode,
    string MachineCode,
    MaintenanceOrderType OrderType,
    string? TriggerRef,
    MaintenancePriority Priority,
    DateTime? PlannedStartAt,
    DateTime? PlannedEndAt,
    string? AssignedTo,
    decimal? EstimatedCost,
    string? Notes,
    string CreatedBy) : ICommand<ValidationResult<int>>;
