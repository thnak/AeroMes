using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.DeleteAdapter;

public record DeleteAdapterCommand(int Id, string? DeletedBy) : ICommand<ValidationResult<int>>;
