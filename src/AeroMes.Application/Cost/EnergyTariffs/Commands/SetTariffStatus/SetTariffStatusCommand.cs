using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Commands.SetTariffStatus;

public record SetTariffStatusCommand(int TariffID, bool Activate) : ICommand<ValidationResult<Unit>>;
