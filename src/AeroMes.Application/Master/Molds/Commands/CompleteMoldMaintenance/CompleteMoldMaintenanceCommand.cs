using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.CompleteMoldMaintenance;

public record CompleteMoldMaintenanceCommand(
    string MoldCode,
    MoldMaintenanceType MaintenanceType,
    DateTime StartDate,
    DateTime? EndDate,
    string? TechnicianId,
    string? Description,
    string? PartReplaced,
    decimal? Cost,
    long? NextDueShots,
    string? UpdatedBy) : ICommand<long>;
