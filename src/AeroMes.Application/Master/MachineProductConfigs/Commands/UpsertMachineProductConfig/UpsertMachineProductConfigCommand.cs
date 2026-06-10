using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.UpsertMachineProductConfig;

public record UpsertMachineProductConfigCommand(
    string MachineCode,
    string ProductCode,
    double IdealCycleTimeSeconds,
    int TargetThroughputPerHour,
    double SetupTimeSeconds,
    DateOnly EffectiveFrom,
    int? RoutingStepId = null) : ICommand;
