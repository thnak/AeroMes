using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineCapacity;

public record UpdateMachineCapacityCommand(
    string Code,
    string? MachineCategory,
    decimal? TargetOeePct,
    decimal? TheoreticalCapacityPerHour,
    int PlannedDowntimeMinPerShift,
    decimal? HourlyCostRate,
    string? OpcUaNodeId,
    bool RequiresCertification,
    string? CertificationCode,
    byte MaxOperators,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
