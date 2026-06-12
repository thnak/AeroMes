using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.DeleteTool;

public record DeleteToolCommand(string ToolCode, string? DeletedBy) : ICommand;
