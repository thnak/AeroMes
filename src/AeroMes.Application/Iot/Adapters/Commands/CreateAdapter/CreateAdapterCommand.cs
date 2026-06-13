using AeroMes.Application.Common;
using AeroMes.Domain.Iot;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.CreateAdapter;

public record CreateAdapterCommand(
    string MachineCode,
    AdapterType AdapterType,
    string ConfigJson,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
