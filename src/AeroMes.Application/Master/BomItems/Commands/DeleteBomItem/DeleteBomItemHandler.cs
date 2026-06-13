using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;

public class DeleteBomItemHandler(
    IBomItemRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteBomItemCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteBomItemCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.BomId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"BomItem '{cmd.BomId}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
