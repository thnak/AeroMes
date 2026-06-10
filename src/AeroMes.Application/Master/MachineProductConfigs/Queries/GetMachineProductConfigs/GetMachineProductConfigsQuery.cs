using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Queries.GetMachineProductConfigs;

public record GetMachineProductConfigsQuery(string MachineCode) : IQuery<IReadOnlyList<MachineProductConfigDto>>;

public record MachineProductConfigDto(
    string MachineCode,
    string ProductCode,
    int? RoutingStepId,
    double IdealCycleTimeSeconds,
    int TargetThroughputPerHour,
    double SetupTimeSeconds,
    DateOnly EffectiveFrom);
