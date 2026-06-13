using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;

public class DeleteWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkCenterCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteWorkCenterCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"WorkCenter '{cmd.Id}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
