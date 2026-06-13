using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolMaintenance;

public record RecordToolMaintenanceCommand(
    string ToolCode,
    ToolMaintenanceType MaintenanceType,
    DateTime PerformedAt,
    string? PerformedBy,
    decimal? Cost,
    int? NextDueCount,
    DateOnly? NextDueDate,
    string? Notes,
    string? UpdatedBy) : ICommand<ValidationResult<long>>;
