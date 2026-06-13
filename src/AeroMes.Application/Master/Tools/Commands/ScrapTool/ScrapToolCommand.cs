using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Tools.Commands.ScrapTool;

public record ScrapToolCommand(string ToolCode, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
