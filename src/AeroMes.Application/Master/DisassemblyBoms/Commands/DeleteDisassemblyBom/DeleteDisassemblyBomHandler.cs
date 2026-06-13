using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.DeleteDisassemblyBom;

public class DeleteDisassemblyBomHandler(
    IDisassemblyBomRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteDisassemblyBomCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteDisassemblyBomCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.DisassemblyBomId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound(
                $"DisassemblyBom '{cmd.DisassemblyBomId}' không tìm thấy.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
