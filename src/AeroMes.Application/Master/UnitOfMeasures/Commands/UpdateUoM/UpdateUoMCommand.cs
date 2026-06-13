using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.UpdateUoM;

public record UpdateUoMCommand(string Code, string Name, string Group) : ICommand<ValidationResult<Unit>>;
