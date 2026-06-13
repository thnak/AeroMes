using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.DeleteMold;

public record DeleteMoldCommand(string MoldCode, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
