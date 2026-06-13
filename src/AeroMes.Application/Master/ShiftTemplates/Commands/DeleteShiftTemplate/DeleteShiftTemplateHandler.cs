using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.DeleteShiftTemplate;

public class DeleteShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteShiftTemplateCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteShiftTemplateCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"ShiftTemplate '{cmd.Code}' was not found.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
