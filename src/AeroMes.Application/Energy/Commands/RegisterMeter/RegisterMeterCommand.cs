using AeroMes.Application.Common;
using AeroMes.Domain.Energy;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.RegisterMeter;

public record RegisterMeterCommand(
    string MeterCode,
    string MeterName,
    UtilityType UtilityType,
    string Unit,
    string? MachineCode,
    int? WorkCenterID,
    bool IsSubMeter,
    int? ParentMeterID,
    int? TariffID,
    string? OpcUaNodeId,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
