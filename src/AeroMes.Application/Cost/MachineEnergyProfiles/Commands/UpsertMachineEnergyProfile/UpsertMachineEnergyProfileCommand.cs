using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.MachineEnergyProfiles.Commands.UpsertMachineEnergyProfile;

public record UpsertMachineEnergyProfileCommand(
    string MachineCode,
    decimal NominalKW,
    decimal LoadFactor,
    int TariffID,
    DateOnly EffectiveFrom) : ICommand<ValidationResult<int>>;
