using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.SendMoldForMaintenance;

public record SendMoldForMaintenanceCommand(
    string MoldCode,
    MoldMaintenanceType MaintenanceType,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
