using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.ScrapTool;

public record ScrapToolCommand(string ToolCode, string? UpdatedBy) : ICommand;
