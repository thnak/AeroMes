using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.ToggleSignal;

public record ToggleSignalCommand(int Id, bool Enabled, string UpdatedBy) : ICommand<ValidationResult<int>>;
