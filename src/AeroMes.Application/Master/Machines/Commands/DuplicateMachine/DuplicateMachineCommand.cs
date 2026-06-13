using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.DuplicateMachine;

public record DuplicateMachineCommand(
    string SourceCode,
    string NewCode,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
