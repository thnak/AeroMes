using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.ToggleAdapter;

public record ToggleAdapterCommand(int Id, bool Enabled, string UpdatedBy) : ICommand<ValidationResult<int>>;
