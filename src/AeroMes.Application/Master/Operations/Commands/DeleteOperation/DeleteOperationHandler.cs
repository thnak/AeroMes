using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public class DeleteOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteOperationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteOperationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Operation '{cmd.Code}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
