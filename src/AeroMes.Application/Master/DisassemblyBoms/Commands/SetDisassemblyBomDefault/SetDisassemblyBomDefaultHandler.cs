using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.SetDisassemblyBomDefault;

public class SetDisassemblyBomDefaultHandler(
    IDisassemblyBomRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<SetDisassemblyBomDefaultCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetDisassemblyBomDefaultCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.DisassemblyBomId, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound(
                    $"DisassemblyBom '{cmd.DisassemblyBomId}' không tìm thấy.");

            var previousDefault = await repo.GetDefaultBySourceProductAsync(entity.SourceProductCode, ct);
            if (previousDefault is not null && previousDefault.DisassemblyBomId != entity.DisassemblyBomId)
                previousDefault.ClearDefault(cmd.UpdatedBy);

            entity.SetAsDefault(cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
