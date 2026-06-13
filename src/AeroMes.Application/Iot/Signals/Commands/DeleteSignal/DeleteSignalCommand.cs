using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.DeleteSignal;

public record DeleteSignalCommand(int Id, string? DeletedBy) : ICommand<ValidationResult<int>>;
