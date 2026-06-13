using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public record DeleteOperationCommand(string Code, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
