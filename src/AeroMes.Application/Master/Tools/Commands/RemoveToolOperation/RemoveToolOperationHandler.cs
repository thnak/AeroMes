using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Tools.Commands.RemoveToolOperation;

public class RemoveToolOperationHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveToolOperationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveToolOperationCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct);
        if (tool is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.ToolCode}' was not found.");

        tool.RemoveOperationMapping(cmd.MappingId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
