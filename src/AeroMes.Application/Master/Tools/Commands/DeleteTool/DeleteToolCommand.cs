using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Tools.Commands.DeleteTool;

public record DeleteToolCommand(string ToolCode, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
