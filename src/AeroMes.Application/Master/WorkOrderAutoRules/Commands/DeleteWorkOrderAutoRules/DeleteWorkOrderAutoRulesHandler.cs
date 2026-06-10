using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.DeleteWorkOrderAutoRules;

public class DeleteWorkOrderAutoRulesHandler(
    IWorkOrderAutoRulesRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkOrderAutoRulesCommand>
{
    public async Task HandleAsync(DeleteWorkOrderAutoRulesCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.RuleId, ct)
            ?? throw new EntityNotFoundException("WorkOrderAutoRules", cmd.RuleId);

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
