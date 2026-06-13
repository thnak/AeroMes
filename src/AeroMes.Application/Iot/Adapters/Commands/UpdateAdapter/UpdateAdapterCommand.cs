using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.UpdateAdapter;

public record UpdateAdapterCommand(
    int Id,
    string ConfigJson,
    string UpdatedBy) : ICommand<ValidationResult<int>>;
