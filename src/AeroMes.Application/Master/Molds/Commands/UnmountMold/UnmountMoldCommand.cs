using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.UnmountMold;

public record UnmountMoldCommand(string MoldCode, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
