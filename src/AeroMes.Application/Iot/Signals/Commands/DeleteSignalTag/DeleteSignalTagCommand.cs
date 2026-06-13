using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.DeleteSignalTag;

public record DeleteSignalTagCommand(string Key) : ICommand<ValidationResult<Unit>>;
