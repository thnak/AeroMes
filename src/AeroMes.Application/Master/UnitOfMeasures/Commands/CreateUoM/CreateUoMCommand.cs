using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.CreateUoM;

public record CreateUoMCommand(
    string Code,
    string Name,
    string Group,
    string? CreatedBy) : ICommand<string>;
