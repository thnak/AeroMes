using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.ReturnTool;

public record ReturnToolCommand(
    string ToolCode,
    string ReturnedBy,
    ToolReturnCondition Condition,
    string? Notes,
    string? UpdatedBy) : ICommand;
