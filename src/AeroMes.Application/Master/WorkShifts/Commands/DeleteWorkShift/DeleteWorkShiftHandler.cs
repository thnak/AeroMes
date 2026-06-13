using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkShifts.Commands.DeleteWorkShift;

public class DeleteWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkShiftCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteWorkShiftCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WorkShiftId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.WorkShiftId}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
