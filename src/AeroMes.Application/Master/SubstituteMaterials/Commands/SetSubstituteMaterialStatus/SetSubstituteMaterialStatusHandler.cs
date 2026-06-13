using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.SetSubstituteMaterialStatus;

public class SetSubstituteMaterialStatusHandler(
    ISubstituteMaterialRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<SetSubstituteMaterialStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SetSubstituteMaterialStatusCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.SubstituteId, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound($"Substitute material {cmd.SubstituteId} not found.");

            if (cmd.IsActive)
                entity.Activate(cmd.UpdatedBy);
            else
                entity.Deactivate(cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
