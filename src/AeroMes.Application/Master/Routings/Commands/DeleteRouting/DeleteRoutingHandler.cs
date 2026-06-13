using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRouting;

public class DeleteRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteRoutingCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteRoutingCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Routing '{cmd.Id}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
