using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.DeleteWorkOrderAutoRules;

public class DeleteWorkOrderAutoRulesHandler(
    IWorkOrderAutoRulesRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkOrderAutoRulesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteWorkOrderAutoRulesCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.RuleId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"WorkOrderAutoRules '{cmd.RuleId}' was not found.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
